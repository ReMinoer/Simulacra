using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class FileFolderWatcher
    {
        private readonly IWatchableFileSystem _fileSystem;
        private readonly object _lock = new object();

        private readonly Dictionary<Delegate, PathHandlersBase> _handlers = new Dictionary<Delegate, PathHandlersBase>();
        private readonly Dictionary<string, SharedWatcher> _sharedWatchers = new Dictionary<string, SharedWatcher>();

        public ILogger Logger { get; set; }

        public FileFolderWatcher()
            : this(new WatchableFileSystem())
        {
        }

        public FileFolderWatcher(IWatchableFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void WatchFile(string path, FileChangedEventHandler handler)
        {
            if (!_fileSystem.IsPathRooted(path) || _fileSystem.IsExplicitFolderPath(path))
                throw new ArgumentException();

            path = _fileSystem.UniqueFile(path);

            lock (_lock)
                _handlers.Add(handler, new FileHandlers(path, GetSharedWatcher(path), handler, LogLevel.Information));
        }

        public void WatchFolder(string path, FolderChangedEventHandler handler)
        {
            if (!_fileSystem.IsPathRooted(path))
                throw new ArgumentException();

            path = _fileSystem.UniqueFolder(path);

            lock (_lock)
                _handlers.Add(handler, new FolderHandlers(path, GetSharedWatcher(path), handler, LogLevel.Information));
        }

        public void Unwatch(FileChangedEventHandler fileHandler) => Unwatch(handler: fileHandler);
        public void Unwatch(FolderChangedEventHandler folderHandler) => Unwatch(handler: folderHandler);
        private void Unwatch(Delegate handler)
        {
            lock (_lock)
            {
                PathHandlersBase pathHandlers = _handlers[handler];
                _handlers.Remove(handler);

                pathHandlers.Release();
            }
        }

        private SharedWatcher GetSharedWatcher(string path)
        {
            path = PathComparer.TransformFolder(path, FolderPathEquality.RespectAmbiguity);

            string folderPath = _fileSystem.GetDirectoryName(path);
            if (folderPath == null)
                return null;

            folderPath = _fileSystem.UniqueFolder(folderPath);

            if (!_sharedWatchers.TryGetValue(folderPath, out SharedWatcher sharedWatcher))
            {
                _sharedWatchers[folderPath] = sharedWatcher = new SharedWatcher(folderPath, this);
                sharedWatcher.FullyReleased += OnSharedWatcherFullyReleased;
            }

            return sharedWatcher;
        }

        private void OnSharedWatcherFullyReleased(string folderPath)
        {
            SharedWatcher sharedWatcher = _sharedWatchers[folderPath];
            _sharedWatchers.Remove(folderPath);

            sharedWatcher.FullyReleased -= OnSharedWatcherFullyReleased;
            sharedWatcher.Dispose();
        }

        private class SharedWatcher : IDisposable
        {
            private int _counter;

            private readonly SharedWatcher _parentWatcher;
            private FolderHandlers _folderHandlers;

            public string FolderPath { get; }
            public FileFolderWatcher FileFolderWatcher { get; }

            public IFileSystemWatcher FileSystemWatcher { get; private set; }
            public IWatchableFileSystem FileSystem => FileFolderWatcher._fileSystem;

            public event Action FolderCreated;
            public event Action FolderDeleted;
            public event Action<string> FullyReleased;

            public SharedWatcher(string folderPath, FileFolderWatcher fileFolderWatcher)
            {
                FolderPath = folderPath;
                FileFolderWatcher = fileFolderWatcher;

                // Can be null if FolderPath is a drive root.
                _parentWatcher = FileFolderWatcher.GetSharedWatcher(FolderPath);

                CreateInternalWatcherIfPossible();
                StartWatchFolder();
                StartWatchParentFolder();
            }

            private void OnParentFolderCreated()
            {
                CreateInternalWatcherIfPossible();
                StartWatchFolder();

                // Notify folder creation if internal watcher has been created
                if (FileSystemWatcher != null)
                    FolderCreated?.Invoke(); 
            }

            private void OnParentFolderDeleted()
            {
                // Notify folder deletion since it existed and parent folder has been deleted
                if (FileSystemWatcher != null)
                    FolderDeleted?.Invoke();

                StopWatchFolder();
                DisposeInternalWatcher();
            }

            private void HandleFolderChange(object sender, FolderChangedEventArgs args)
            {
                switch (args.ChangeType)
                {
                    case FolderChangeType.Created:

                        CreateInternalWatcherIfPossible();
                        FolderCreated?.Invoke();

                        break;
                    case FolderChangeType.Deleted:

                        FolderDeleted?.Invoke();
                        DisposeInternalWatcher();

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            public void Increment() => _counter++;
            public void Release()
            {
                _counter--;
                if (_counter <= 0)
                    FullyReleased?.Invoke(FolderPath);
            }

            public void Dispose()
            {
                StopWatchParentFolder();
                StopWatchFolder();
                DisposeInternalWatcher();
            }

            private void CreateInternalWatcherIfPossible()
            {
                if (FileSystemWatcher == null)
                    FileSystemWatcher = FileSystem.GetWatcher(FolderPath);
            }

            private void DisposeInternalWatcher()
            {
                if (FileSystemWatcher != null)
                {
                    FileSystemWatcher.EnableRaisingEvents = false;
                    FileSystemWatcher.Dispose();
                    FileSystemWatcher = null;
                }
            }

            private void StartWatchFolder()
            {
                if (_folderHandlers == null && _parentWatcher != null)
                    _folderHandlers = new FolderHandlers(FolderPath, _parentWatcher, HandleFolderChange, LogLevel.Debug);
            }

            private void StopWatchFolder()
            {
                if (_folderHandlers != null)
                {
                    _folderHandlers.Release();
                    _folderHandlers = null;
                }
            }

            private void StartWatchParentFolder()
            {
                if (_parentWatcher != null)
                {
                    _parentWatcher.FolderCreated += OnParentFolderCreated;
                    _parentWatcher.FolderDeleted += OnParentFolderDeleted;
                }
            }

            private void StopWatchParentFolder()
            {
                if (_parentWatcher != null)
                {
                    _parentWatcher.FolderDeleted -= OnParentFolderDeleted;
                    _parentWatcher.FolderCreated -= OnParentFolderCreated;
                }
            }
        }

        private abstract class PathHandlersBase
        {
            private readonly bool _subscribeToChanged;
            private bool _existedLastly;
            private bool _mightBeQuicklyCreatedAfterDelete;
            private bool _mightBeQuicklyDeletedAfterCreate;

            public string Path { get; }
            public SharedWatcher SharedWatcher { get; }
            protected ILogger Logger => SharedWatcher.FileFolderWatcher.Logger;

            public PathHandlersBase(string path, SharedWatcher sharedWatcher, bool subscribeToChanged)
            {
                Path = path;
                SharedWatcher = sharedWatcher;
                _subscribeToChanged = subscribeToChanged;

                SharedWatcher.Increment();
                SharedWatcher.FolderCreated += OnParentFolderCreated;
                SharedWatcher.FolderDeleted += OnParentFolderDeleted;
                SubscribeToWatcher();
            }

            private void OnParentFolderCreated()
            {
                SubscribeToWatcher();
                if (PathExist())
                {
                    _existedLastly = true;
                    NotifyCreated();
                }
            }

            private void OnParentFolderDeleted()
            {
                UnsubscribeToWatcher();
                if (_existedLastly)
                {
                    _existedLastly = false;
                    NotifyDeleted();
                }
            }

            private void SubscribeToWatcher()
            {
                _existedLastly = PathExist();

                IFileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                if (fileSystemWatcher == null)
                    return;

                if (_subscribeToChanged)
                    fileSystemWatcher.Changed += OnChanged;
                fileSystemWatcher.Created += OnCreated;
                fileSystemWatcher.Deleted += OnDeleted;
                fileSystemWatcher.Renamed += OnRenamed;
                fileSystemWatcher.EnableRaisingEvents = true;
            }

            private void UnsubscribeToWatcher()
            {
                IFileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                if (fileSystemWatcher == null)
                    return;

                fileSystemWatcher.Renamed -= OnRenamed;
                fileSystemWatcher.Deleted -= OnDeleted;
                fileSystemWatcher.Created -= OnCreated;
                if (_subscribeToChanged)
                    fileSystemWatcher.Changed -= OnChanged;
            }

            public void Release()
            {
                UnsubscribeToWatcher();
                SharedWatcher.Release();
            }

            protected virtual void NotifyEdited() => throw new InvalidOperationException();
            protected abstract void NotifyCreated();
            protected abstract void NotifyDeleted();
            protected abstract bool PathExist();

            private void OnChanged(object sender, System.IO.FileSystemEventArgs e)
            {
                if (MatchExisting(e.FullPath))
                    NotifyEdited();
            }

            private void OnCreated(object sender, System.IO.FileSystemEventArgs e)
            {
                if (MatchCreated(e.FullPath))
                {
                    _existedLastly = true;
                    NotifyCreated();
                }
            }

            private void OnDeleted(object sender, System.IO.FileSystemEventArgs e)
            {
                if (MatchDeleted(e.FullPath))
                {
                    _existedLastly = false;
                    NotifyDeleted();
                }
            }

            private void OnRenamed(object sender, System.IO.RenamedEventArgs e)
            {
                if (MatchDeleted(e.OldFullPath))
                {
                    _existedLastly = false;
                    NotifyDeleted();
                }
                else if (MatchCreated(e.FullPath))
                {
                    _existedLastly = true;
                    NotifyCreated();
                }
            }

            private bool EqualsPath(string path) => PathComparer.Equals(path, Path, PathCaseComparison.EnvironmentDefault, FolderPathEquality.RespectAmbiguity);

            protected bool MatchExisting(string path) => EqualsPath(path) && PathExist();
            protected bool MatchCreated(string path)
            {
                // Check path
                if (EqualsPath(path))
                {
                    // Also check file/folder DO exists since we might be watching for a folder/file with same path.
                    if (PathExist())
                    {
                        // Notify DELETED if we had doubts previously
                        if (_mightBeQuicklyCreatedAfterDelete)
                        {
                            NotifyDeleted();
                            _mightBeQuicklyCreatedAfterDelete = false;
                        }

                        return true;
                    }

                    // If file/folder DOES NOT exist, there is two possibility:
                    //   1. The path might be used by a different entry type (folder/file).
                    //   2. The entry might have been quickly DELETED after CREATE.
                    // We will wait next events since second case will trigger DELETED after that.
                    _mightBeQuicklyDeletedAfterCreate = true;
                }

                // Other case can be reset
                _mightBeQuicklyCreatedAfterDelete = false;
                return false;
            }

            protected bool MatchDeleted(string path)
            {
                // Check path
                if (EqualsPath(path))
                {
                    // Also check file/folder DOES NOT exists since we might be watching for a folder/file with same path.
                    if (!PathExist())
                    {
                        // Notify CREATED if we had doubts previously
                        if (_mightBeQuicklyDeletedAfterCreate)
                        {
                            NotifyCreated();
                            _mightBeQuicklyDeletedAfterCreate = false;
                        }

                        return true;
                    }

                    // If file/folder DO exist, there is two possibility:
                    //   1. The path might be used by a different entry type (folder/file).
                    //   2. The entry might have been quickly CREATED after DELETE.
                    // We will wait next events since second case will trigger CREATED after that.
                    _mightBeQuicklyCreatedAfterDelete = true;
                }

                // Other case can be reset
                _mightBeQuicklyDeletedAfterCreate = false;
                return false;
            }
        }

        private class FileHandlers : PathHandlersBase
        {
            private readonly FileChangedEventHandler _handler;
            private readonly LogLevel _logLevel;

            public FileHandlers(string path, SharedWatcher sharedWatcher, FileChangedEventHandler handler, LogLevel logLevel)
                : base(path, sharedWatcher, subscribeToChanged: true)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist() => SharedWatcher.FileSystem.FileExists(Path);
            protected override void NotifyEdited() => Notify(FileChangeType.Edited);
            protected override void NotifyCreated() => Notify(FileChangeType.Created);
            protected override void NotifyDeleted() => Notify(FileChangeType.Deleted);

            private void Notify(FileChangeType changeType)
            {
                _handler(SharedWatcher.FileFolderWatcher, new FileChangedEventArgs(Path, changeType));
                Logger?.Log(_logLevel, $"{changeType} file: {Path}");
            }
        }

        private class FolderHandlers : PathHandlersBase
        {
            private readonly FolderChangedEventHandler _handler;
            private readonly LogLevel _logLevel;

            public FolderHandlers(string path, SharedWatcher sharedWatcher, FolderChangedEventHandler handler, LogLevel logLevel)
                : base(path, sharedWatcher, subscribeToChanged: false)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist() => SharedWatcher.FileSystem.FolderExists(Path);
            protected override void NotifyCreated() => Notify(FolderChangeType.Created);
            protected override void NotifyDeleted() => Notify(FolderChangeType.Deleted);

            private void Notify(FolderChangeType changeType)
            {
                _handler(SharedWatcher.FileFolderWatcher, new FolderChangedEventArgs(Path, changeType));
                Logger?.Log(_logLevel, $"{changeType} folder: {Path}");
            }
        }
    }
}
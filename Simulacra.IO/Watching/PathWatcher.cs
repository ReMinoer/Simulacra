using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class PathWatcher
    {
        private readonly IWatchableFileSystem _fileSystem;
        private readonly object _lock = new object();

        private readonly Dictionary<Delegate, PathHandlersBase> _handlers = new Dictionary<Delegate, PathHandlersBase>();
        private readonly Dictionary<string, SharedWatcher> _sharedWatchers = new Dictionary<string, SharedWatcher>();

        public ILogger Logger { get; set; }

        public PathWatcher()
            : this(WatchableFileSystem.Instance)
        {
        }

        public PathWatcher(IWatchableFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void WatchFile(string path, FileChangedEventHandler handler)
        {
            if (!_fileSystem.IsPathRooted(path) || _fileSystem.IsExplicitFolderPath(path))
                throw new ArgumentException();

            string uniquePath = _fileSystem.UniqueFile(path);
            SharedWatcher sharedWatcher = GetSharedWatcher(uniquePath);
            if (sharedWatcher == null)
                throw new ArgumentException();

            lock (_lock)
                _handlers.Add(handler, new FileHandlers(uniquePath, sharedWatcher, handler, LogLevel.Information));

            Logger?.LogInformation($"Start watching file: {uniquePath}");
        }

        public void WatchFolder(string path, FolderChangedEventHandler handler)
        {
            if (!_fileSystem.IsPathRooted(path))
                throw new ArgumentException();

            string uniquePath = _fileSystem.UniqueFolder(path);
            SharedWatcher sharedWatcher = GetSharedWatcher(uniquePath);
            if (sharedWatcher == null)
                throw new ArgumentException();

            lock (_lock)
                _handlers.Add(handler, new FolderHandlers(uniquePath, sharedWatcher, handler, LogLevel.Information));

            Logger?.LogInformation($"Start watching folder: {uniquePath}");
        }

        public void Unwatch(FileChangedEventHandler fileHandler) => Unwatch(handler: fileHandler);
        public void Unwatch(FolderChangedEventHandler folderHandler) => Unwatch(handler: folderHandler);
        private void Unwatch(Delegate handler)
        {
            lock (_lock)
            {
                PathHandlersBase pathHandlers = _handlers[handler];
                _handlers.Remove(handler);
                Logger?.LogDebug($"Stop watching file: {pathHandlers.UniquePath}");

                pathHandlers.Release();
            }
        }

        private SharedWatcher GetSharedWatcher(string uniquePath)
        {
            string folderPath = _fileSystem.GetFolderPath(uniquePath);
            if (folderPath == null)
                return null;

            if (!_sharedWatchers.TryGetValue(uniquePath, out SharedWatcher sharedWatcher))
            {
                _sharedWatchers[uniquePath] = sharedWatcher = new SharedWatcher(uniquePath, this);
                sharedWatcher.FullyReleased += OnSharedWatcherFullyReleased;

                Logger?.LogDebug($"Create shared watcher for: {uniquePath}");
            }

            return sharedWatcher;
        }

        private void OnSharedWatcherFullyReleased(object sender, EventArgs e)
        {
            SharedWatcher sharedWatcher = _sharedWatchers.First(x => x.Value == sender).Value;
            string uniquePath = sharedWatcher.UniquePath;

            _sharedWatchers.Remove(uniquePath);

            sharedWatcher.FullyReleased -= OnSharedWatcherFullyReleased;
            sharedWatcher.Dispose();

            Logger?.LogDebug($"Fully release shared watcher for: {uniquePath}");
        }

        private class SharedWatcher : IShared, IDisposable
        {
            private int _counter;

            private readonly SharedWatcher _parentWatcher;
            private FolderHandlers _folderHandlers;

            public string UniquePath { get; }
            public string FolderPath { get; }
            public PathWatcher PathWatcher { get; }

            public IFileSystemWatcher FileSystemWatcher { get; private set; }
            public IWatchableFileSystem FileSystem => PathWatcher._fileSystem;

            public event Action FolderCreated;
            public event Action FolderDeleted;
            public event EventHandler FullyReleased;

            public SharedWatcher(string uniquePath, PathWatcher pathWatcher)
            {
                UniquePath = uniquePath;
                PathWatcher = pathWatcher;

                string folderPath = FileSystem.GetFolderPath(uniquePath);
                if (folderPath == null)
                    throw new InvalidOperationException();

                FolderPath = FileSystem.UniqueFolder(folderPath);
                _parentWatcher = PathWatcher.GetSharedWatcher(FolderPath);

                TryGetInternalWatcher();
                StartWatchFolder();
                StartWatchParentFolder();
            }

            private void OnParentFolderCreated()
            {
                TryGetInternalWatcher();

                // Notify folder creation if internal watcher has been created (meaning folder exists)
                if (FileSystemWatcher != null)
                    FolderCreated?.Invoke(); 
            }

            private void OnParentFolderDeleted()
            {
                // Notify folder deletion since it existed and parent folder has been deleted
                if (FileSystemWatcher != null)
                    FolderDeleted?.Invoke();

                DisposeInternalWatcher();
            }

            private void HandleFolderChange(object sender, FolderChangedEventArgs args)
            {
                switch (args.ChangeType)
                {
                    case FolderChangeType.Created:

                        TryGetInternalWatcher();
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
                    FullyReleased?.Invoke(this, EventArgs.Empty);
            }

            public void Dispose()
            {
                StopWatchParentFolder();
                StopWatchFolder();
                DisposeInternalWatcher();
            }

            private void TryGetInternalWatcher()
            {
                if (FileSystemWatcher == null)
                {
                    FileSystemWatcher = FileSystem.GetWatcher(UniquePath);
                    FileSystemWatcher?.Increment();
                }
            }

            private void DisposeInternalWatcher()
            {
                if (FileSystemWatcher != null)
                {
                    FileSystemWatcher.Release();
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
            private bool _subscribed;
            private readonly bool _subscribeToChanged;
            private bool _existedLastly;
            private bool _mightBeQuicklyCreatedAfterDelete;
            private bool _mightBeQuicklyDeletedAfterCreate;

            public string UniquePath { get; }
            public SharedWatcher SharedWatcher { get; }
            protected IWatchableFileSystem FileSystem => SharedWatcher.FileSystem;
            protected ILogger Logger => SharedWatcher.PathWatcher.Logger;

            public PathHandlersBase(string uniquePath, SharedWatcher sharedWatcher, bool subscribeToChanged)
            {
                UniquePath = uniquePath;
                SharedWatcher = sharedWatcher;
                _subscribeToChanged = subscribeToChanged;

                SharedWatcher.Increment();
                SharedWatcher.FolderCreated += OnParentFolderCreated;
                SharedWatcher.FolderDeleted += OnParentFolderDeleted;

                _existedLastly = PathExist();
                SubscribeToWatcher();
            }

            private void OnParentFolderCreated()
            {
                if (!_existedLastly && PathExist())
                {
                    _existedLastly = true;
                    NotifyCreated();
                }

                SubscribeToWatcher();
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
                if (_subscribed)
                    return;

                IFileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                if (fileSystemWatcher == null)
                    return;

                if (_subscribeToChanged)
                    fileSystemWatcher.Changed += OnChanged;
                fileSystemWatcher.Created += OnCreated;
                fileSystemWatcher.Deleted += OnDeleted;
                fileSystemWatcher.Renamed += OnRenamed;
                fileSystemWatcher.Enable();

                _subscribed = true;
            }

            private void UnsubscribeToWatcher()
            {
                if (!_subscribed)
                    return;

                IFileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                if (fileSystemWatcher == null)
                    return;

                fileSystemWatcher.Renamed -= OnRenamed;
                fileSystemWatcher.Deleted -= OnDeleted;
                fileSystemWatcher.Created -= OnCreated;
                if (_subscribeToChanged)
                    fileSystemWatcher.Changed -= OnChanged;

                _subscribed = false;
            }

            public void Release()
            {
                UnsubscribeToWatcher();
                SharedWatcher.Release();
            }

            protected virtual void NotifyEdited() => throw new InvalidOperationException();
            protected abstract void NotifyCreated(string oldPath = null);
            protected abstract void NotifyDeleted(string newPath = null);
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
                    NotifyDeleted(newPath: e.FullPath);
                }
                else if (MatchCreated(e.FullPath))
                {
                    _existedLastly = true;
                    NotifyCreated(oldPath: e.OldFullPath);
                }
            }

            private bool EqualsPath(string path)
            {
                PathCaseComparison caseComparison = FileSystem.PathsCaseSensitive ? PathCaseComparison.RespectCase : PathCaseComparison.IgnoreCase;
                return PathComparer.Equals(path, UniquePath, caseComparison, FolderPathEquality.RespectAmbiguity);
            }

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

            public FileHandlers(string uniquePath, SharedWatcher sharedWatcher, FileChangedEventHandler handler, LogLevel logLevel)
                : base(uniquePath, sharedWatcher, subscribeToChanged: true)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist() => SharedWatcher.FileSystem.FileExists(UniquePath);
            protected override void NotifyEdited() => Notify(FileChangeType.Edited);
            protected override void NotifyCreated(string oldPath = null) => Notify(FileChangeType.Created, oldPath: oldPath);
            protected override void NotifyDeleted(string newPath = null) => Notify(FileChangeType.Deleted, newPath: newPath);

            private void Notify(FileChangeType changeType, string newPath = null, string oldPath = null)
            {
                _handler(SharedWatcher.PathWatcher, new FileChangedEventArgs(UniquePath, changeType, newPath, oldPath));
                Logger?.Log(_logLevel, $"{changeType} file: {UniquePath}");
            }
        }

        private class FolderHandlers : PathHandlersBase
        {
            private readonly FolderChangedEventHandler _handler;
            private readonly LogLevel _logLevel;

            public FolderHandlers(string uniquePath, SharedWatcher sharedWatcher, FolderChangedEventHandler handler, LogLevel logLevel)
                : base(uniquePath, sharedWatcher, subscribeToChanged: false)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist() => SharedWatcher.FileSystem.FolderExists(UniquePath);
            protected override void NotifyCreated(string oldPath = null) => Notify(FolderChangeType.Created, oldPath: oldPath);
            protected override void NotifyDeleted(string newPath = null) => Notify(FolderChangeType.Deleted, newPath: newPath);

            private void Notify(FolderChangeType changeType, string newPath = null, string oldPath = null)
            {
                _handler(SharedWatcher.PathWatcher, new FolderChangedEventArgs(UniquePath, changeType, newPath, oldPath));
                Logger?.Log(_logLevel, $"{changeType} folder: {UniquePath}");
            }
        }
    }
}
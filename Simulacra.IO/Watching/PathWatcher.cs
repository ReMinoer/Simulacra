using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class PathWatcher
    {
        private readonly IFileSystemWatcherProvider _watcherProvider;
        private readonly object _lock = new object();

        private readonly Dictionary<(string, Delegate), PathHandlersBase> _handlers = new Dictionary<(string, Delegate), PathHandlersBase>();
        private readonly Dictionary<string, SharedWatcher> _sharedWatchers = new Dictionary<string, SharedWatcher>();
        
        public IWatchableFileSystem FileSystem { get; }
        public ILogger Logger { get; set; }

        public PathWatcher()
            : this(WatchableFileSystem.Instance, new OptimizedWatcherProvider(WatchableFileSystem.Instance))
        {
        }

        public PathWatcher(IWatchableFileSystem fileSystem, IFileSystemWatcherProvider watcherProvider)
        {
            FileSystem = fileSystem;
            _watcherProvider = watcherProvider;
        }

        public void WatchFile(string path, FileChangedEventHandler handler)
        {
            if (!FileSystem.IsPathRooted(path) || FileSystem.IsExplicitFolderPath(path) || !PathPattern.IsValid(path, FileSystem))
                throw new ArgumentException();

            string uniquePath = FileSystem.UniqueFile(path);
            SharedWatcher sharedWatcher = GetSharedWatcher(uniquePath);
            if (sharedWatcher == null)
                throw new ArgumentException();

            Logger?.LogInformation($"Start watching file: {uniquePath}");

            lock (_lock)
                _handlers.Add((uniquePath, handler), new FileHandlers(uniquePath, sharedWatcher, handler, LogLevel.Information));
        }

        public void WatchFolder(string path, FolderChangedEventHandler handler)
        {
            if (!FileSystem.IsPathRooted(path) || !PathPattern.IsValid(path, FileSystem))
                throw new ArgumentException();

            string uniquePath = FileSystem.UniqueFolder(path);
            SharedWatcher sharedWatcher = GetSharedWatcher(uniquePath);
            if (sharedWatcher == null)
                throw new ArgumentException();

            Logger?.LogInformation($"Start watching folder: {uniquePath}");

            lock (_lock)
                _handlers.Add((uniquePath, handler), new FolderHandlers(uniquePath, sharedWatcher, handler, LogLevel.Information));
        }

        public void Unwatch(string path, FileChangedEventHandler fileHandler)
        {
            string uniquePath = FileSystem.UniqueFile(path);
            Logger?.LogInformation($"Stop watching file: {uniquePath}");

            Unwatch(uniquePath, handler: fileHandler);
        }

        public void Unwatch(string path, FolderChangedEventHandler folderHandler)
        {
            string uniquePath = FileSystem.UniqueFolder(path);
            Logger?.LogInformation($"Stop watching folder: {uniquePath}");

            Unwatch(uniquePath, handler: folderHandler);
        }

        private void Unwatch(string uniquePath, Delegate handler)
        {
            lock (_lock)
            {
                (string, Delegate) key = (uniquePath, handler);
                PathHandlersBase pathHandlers = _handlers[key];
                _handlers.Remove(key);

                pathHandlers.Release();
            }
        }

        public IDisposable SuspendWatching(string path, FileChangedEventHandler fileHandler) => SuspendWatching(FileSystem.UniqueFile(path), handler: fileHandler);
        public IDisposable SuspendWatching(string path, FolderChangedEventHandler folderHandler) => SuspendWatching(FileSystem.UniqueFolder(path), handler: folderHandler);
        private IDisposable SuspendWatching(string uniquePath, Delegate handler)
        {
            lock (_lock)
            {
                (string, Delegate) key = (uniquePath, handler);
                PathHandlersBase pathHandlers = _handlers[key];

                return new SuspendWatchingDisposable(pathHandlers);
            }
        }

        private class SuspendWatchingDisposable : IDisposable
        {
            private readonly PathHandlersBase _pathHandlers;

            public SuspendWatchingDisposable(PathHandlersBase pathHandlers)
            {
                pathHandlers.Suspend();
                _pathHandlers = pathHandlers;
            }

            public void Dispose()
            {
                _pathHandlers.Resume();
            }
        }

        private SharedWatcher GetSharedWatcher(string uniquePath)
        {
            string folderPath = FileSystem.GetFolderPath(uniquePath);
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
            public IWatchableFileSystem FileSystem => PathWatcher.FileSystem;
            public IFileSystemWatcherProvider WatcherProvider => PathWatcher._watcherProvider;

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
                    FileSystemWatcher = WatcherProvider.GetWatcher(UniquePath);
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

            private readonly object _lock = new object();
            private int _suspendCounter;
            private bool IsSuspended => _suspendCounter > 0;

            private readonly ConcurrentDictionary<string, bool> _matchingPaths;
            private bool _mightBeQuicklyCreatedAfterDelete;
            private bool _mightBeQuicklyDeletedAfterCreate;

            public string UniquePath { get; }
            public PathPattern PathPattern { get; }
            public SharedWatcher SharedWatcher { get; }

            protected ILogger Logger => SharedWatcher.PathWatcher.Logger;
            protected abstract string LoggedEntryType { get; }

            public PathHandlersBase(string uniquePath, SharedWatcher sharedWatcher, bool subscribeToChanged)
            {
                UniquePath = uniquePath;
                PathPattern = new PathPattern(uniquePath, sharedWatcher.FileSystem, sharedWatcher.FileSystem.PathsCaseSensitive);

                SharedWatcher = sharedWatcher;
                _subscribeToChanged = subscribeToChanged;

                SharedWatcher.Increment();
                SharedWatcher.FolderCreated += OnParentFolderCreated;
                SharedWatcher.FolderDeleted += OnParentFolderDeleted;

                PathCaseComparison caseComparison = sharedWatcher.FileSystem.PathsCaseSensitive
                    ? PathCaseComparison.RespectCase
                    : PathCaseComparison.IgnoreCase;

                var existingPathComparer = new PathComparer(sharedWatcher.FileSystem, caseComparison, FolderPathEquality.RespectAmbiguity);
                _matchingPaths = new ConcurrentDictionary<string, bool>(existingPathComparer);

                RefreshExistingMatchingPaths();
                SubscribeToWatcher();
            }

            protected abstract bool PathExist(string path);
            protected abstract IEnumerable<string> GetExistingMatchingPaths();

            protected virtual void NotifyEdited(string path) => throw new InvalidOperationException();
            protected abstract void NotifyCreated(string path, string oldPath = null);
            protected abstract void NotifyDeleted(string path, string newPath = null);

            private void OnParentFolderCreated()
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    foreach (string createdPath in GetExistingMatchingPaths().Where(TryRegisterMatchingPath))
                        NotifyCreated(createdPath);

                    SubscribeToWatcher();
                }
            }

            private void OnParentFolderDeleted()
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    UnsubscribeToWatcher();

                    foreach (string pathToDelete in _matchingPaths.Keys.Where(TryUnregisterMatchingPath))
                        NotifyDeleted(pathToDelete);

                    if (_matchingPaths.Count > 0)
                        throw new InvalidOperationException();
                }
            }

            private void RefreshExistingMatchingPaths()
            {
                _matchingPaths.Clear();

                foreach (string existingMatchingPath in GetExistingMatchingPaths())
                    RegisterMatchingPath(existingMatchingPath);
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
                lock (_lock)
                {
                    UnsubscribeToWatcher();
                    SharedWatcher.Release();
                }
            }

            private void OnChanged(object sender, System.IO.FileSystemEventArgs e)
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    if (MatchExisting(e.FullPath))
                        NotifyEdited(e.FullPath);
                }
            }

            private void OnCreated(object sender, System.IO.FileSystemEventArgs e)
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    if (MatchCreated(e.FullPath) && TryRegisterMatchingPath(e.FullPath))
                        NotifyCreated(e.FullPath);
                }
            }

            private void OnDeleted(object sender, System.IO.FileSystemEventArgs e)
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    if (MatchDeleted(e.FullPath) && TryUnregisterMatchingPath(e.FullPath))
                        NotifyDeleted(e.FullPath);
                }
            }

            private void OnRenamed(object sender, System.IO.RenamedEventArgs e)
            {
                lock (_lock)
                {
                    if (IsSuspended)
                        return;

                    if (MatchDeleted(e.OldFullPath) && TryUnregisterMatchingPath(e.OldFullPath))
                        NotifyDeleted(e.OldFullPath, newPath: e.FullPath);
                    else if (MatchCreated(e.FullPath) && TryRegisterMatchingPath(e.FullPath))
                        NotifyCreated(e.FullPath, oldPath: e.OldFullPath);
                }
            }

            private void RegisterMatchingPath(string path)
            {
                if (!TryRegisterMatchingPath(path))
                    throw new InvalidOperationException();
            }

            private void UnregisterMatchingPath(string path)
            {
                if (!TryUnregisterMatchingPath(path))
                    throw new InvalidOperationException();
            }

            private bool TryRegisterMatchingPath(string path)
            {
                Logger?.LogDebug($"Found path {path} (matching {UniquePath})");
                return _matchingPaths.TryAdd(path, true);
            }

            private bool TryUnregisterMatchingPath(string path)
            {
                Logger?.LogInformation($"Forget path {path} (matching {UniquePath})");
                return _matchingPaths.TryRemove(path, out _);
            }

            protected bool MatchExisting(string path) => PathPattern.Match(path) && PathExist(path);
            protected bool MatchCreated(string path)
            {
                // Check path
                if (PathPattern.Match(path))
                {
                    // Also check file/folder DO exists since we might be watching for a folder/file with same path.
                    if (PathExist(path))
                    {
                        // Notify DELETED if we had doubts previously
                        if (_mightBeQuicklyCreatedAfterDelete)
                        {
                            UnregisterMatchingPath(path);
                            NotifyDeleted(path);
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
                if (PathPattern.Match(path))
                {
                    // Also check file/folder DOES NOT exists since we might be watching for a folder/file with same path.
                    if (!PathExist(path))
                    {
                        // Notify CREATED if we had doubts previously
                        if (_mightBeQuicklyDeletedAfterCreate)
                        {
                            RegisterMatchingPath(path);
                            NotifyCreated(path);
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

            public void Suspend()
            {
                lock (_lock)
                {
                    if (_suspendCounter == 0)
                    {
                        Logger?.LogInformation($"Suspend watching {LoggedEntryType}: {UniquePath}");
                        UnsubscribeToWatcher();
                    }

                    _suspendCounter++;
                }
            }

            public void Resume()
            {
                lock (_lock)
                {
                    _suspendCounter--;

                    if (_suspendCounter == 0)
                    {
                        Logger?.LogInformation($"Resume watching {LoggedEntryType}: {UniquePath}");
                        SubscribeToWatcher();
                        RefreshExistingMatchingPaths();
                    }
                }
            }
        }

        private class FileHandlers : PathHandlersBase
        {
            private readonly FileChangedEventHandler _handler;
            private readonly LogLevel _logLevel;

            protected override string LoggedEntryType => "file";

            public FileHandlers(string uniquePath, SharedWatcher sharedWatcher, FileChangedEventHandler handler, LogLevel logLevel)
                : base(uniquePath, sharedWatcher, subscribeToChanged: true)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist(string path) => SharedWatcher.FileSystem.FileExists(path);
            protected override IEnumerable<string> GetExistingMatchingPaths() => SharedWatcher.FileSystem.GetFiles(PathPattern);

            protected override void NotifyEdited(string path) => Notify(FileChangeType.Edited, path);
            protected override void NotifyCreated(string path, string oldPath = null) => Notify(FileChangeType.Created, path, oldPath: oldPath);
            protected override void NotifyDeleted(string path, string newPath = null) => Notify(FileChangeType.Deleted, path, newPath: newPath);

            private void Notify(FileChangeType changeType, string path, string newPath = null, string oldPath = null)
            {
                _handler(SharedWatcher.PathWatcher, new FileChangedEventArgs(PathPattern, path, changeType, newPath, oldPath));
                Logger?.Log(_logLevel, $"{changeType} file: {path} (matching {UniquePath})");
            }
        }

        private class FolderHandlers : PathHandlersBase
        {
            private readonly FolderChangedEventHandler _handler;
            private readonly LogLevel _logLevel;

            protected override string LoggedEntryType => "folder";

            public FolderHandlers(string uniquePath, SharedWatcher sharedWatcher, FolderChangedEventHandler handler, LogLevel logLevel)
                : base(uniquePath, sharedWatcher, subscribeToChanged: false)
            {
                _handler = handler;
                _logLevel = logLevel;
            }

            protected override bool PathExist(string path) => SharedWatcher.FileSystem.FolderExists(path);
            protected override IEnumerable<string> GetExistingMatchingPaths() => SharedWatcher.FileSystem.GetFolders(PathPattern);

            protected override void NotifyCreated(string path, string oldPath = null) => Notify(FolderChangeType.Created, path, oldPath: oldPath);
            protected override void NotifyDeleted(string path, string newPath = null) => Notify(FolderChangeType.Deleted, path, newPath: newPath);

            private void Notify(FolderChangeType changeType, string path, string newPath = null, string oldPath = null)
            {
                _handler(SharedWatcher.PathWatcher, new FolderChangedEventArgs(PathPattern, path, changeType, newPath, oldPath));
                Logger?.Log(_logLevel, $"{changeType} folder: {path} (matching {UniquePath})");
            }
        }
    }
}
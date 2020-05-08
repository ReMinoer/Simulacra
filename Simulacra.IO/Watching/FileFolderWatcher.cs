using System;
using System.Collections.Generic;
using System.IO;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class FileFolderWatcher
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Delegate, PathHandlersBase> _handlers = new Dictionary<Delegate, PathHandlersBase>();
        private readonly Dictionary<string, SharedWatcher> _sharedWatchers = new Dictionary<string, SharedWatcher>();

        public void Subscribe(string path, FileChangedEventHandler handler)
        {
            if (!Path.IsPathRooted(path))
                throw new ArgumentException();

            lock (_lock)
                _handlers.Add(handler, new FileHandlers(PathUtils.Normalize(path), GetSharedWatcher(path), handler));
        }

        public void Subscribe(string path, FolderChangedEventHandler handler)
        {
            if (!Path.IsPathRooted(path))
                throw new ArgumentException();

            lock (_lock)
                _handlers.Add(handler, new FolderHandlers(PathUtils.NormalizeFolder(path), GetSharedWatcher(path), handler));
        }

        public void Unsubscribe(FileChangedEventHandler fileHandler) => Unsubscribe(handler: fileHandler);
        public void Unsubscribe(FolderChangedEventHandler folderHandler) => Unsubscribe(handler: folderHandler);
        private void Unsubscribe(Delegate handler)
        {
            lock (_lock)
            {
                PathHandlersBase pathHandlers = _handlers[handler];
                _handlers.Remove(handler);

                SharedWatcher sharedWatcher = pathHandlers.SharedWatcher;
                pathHandlers.Release(sharedWatcher, out bool watcherNotUsedAnymore);

                if (watcherNotUsedAnymore)
                    _sharedWatchers.Remove(sharedWatcher.FolderPath);
            }
        }

        private SharedWatcher GetSharedWatcher(string path)
        {
            string folderPath = Path.GetDirectoryName(path) ?? throw new DirectoryNotFoundException();

            if (!_sharedWatchers.TryGetValue(folderPath, out SharedWatcher sharedWatcher))
                _sharedWatchers[folderPath] = sharedWatcher = new SharedWatcher(folderPath);

            return sharedWatcher;
        }

        private class SharedWatcher
        {
            private int _counter;
            public string FolderPath { get; }
            public FileSystemWatcher FileSystemWatcher { get; }

            public SharedWatcher(string folderPath)
            {
                FolderPath = folderPath;
                FileSystemWatcher = new FileSystemWatcher(folderPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
                };
            }

            public void Increment() => _counter++;
            public bool Release()
            {
                _counter--;
                if (_counter > 0)
                    return false;

                FileSystemWatcher.EnableRaisingEvents = false;
                FileSystemWatcher.Dispose();
                return true;
            }
        }

        private abstract class PathHandlersBase
        {
            public string Path { get; }
            public SharedWatcher SharedWatcher { get; }

            public PathHandlersBase(string path, SharedWatcher sharedWatcher)
            {
                Path = path;
                SharedWatcher = sharedWatcher;

                sharedWatcher.Increment();
                sharedWatcher.FileSystemWatcher.EnableRaisingEvents = true;
            }

            protected abstract void Unsubscribe();

            public void Release(SharedWatcher sharedWatcher, out bool watcherNotUsedAnymore)
            {
                watcherNotUsedAnymore = sharedWatcher.Release();
                Unsubscribe();
            }
        }

        private class FileHandlers : PathHandlersBase
        {
            private readonly FileChangedEventHandler _handler;

            public FileHandlers(string path, SharedWatcher sharedWatcher, FileChangedEventHandler handler)
                : base(path, sharedWatcher)
            {
                _handler = handler;

                FileSystemWatcher fileSystemWatcher = sharedWatcher.FileSystemWatcher;
                fileSystemWatcher.Changed += OnChanged;
                fileSystemWatcher.Created += OnCreated;
                fileSystemWatcher.Deleted += OnDeleted;
                fileSystemWatcher.Renamed += OnRenamed;
            }

            protected override void Unsubscribe()
            {
                FileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                fileSystemWatcher.Renamed -= OnRenamed;
                fileSystemWatcher.Deleted -= OnDeleted;
                fileSystemWatcher.Created -= OnCreated;
                fileSystemWatcher.Changed -= OnChanged;
            }

            private void OnChanged(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == Path)
                    _handler(this, new FileChangedEventArgs(Path, FileChangeType.Edited));
            }

            private void OnCreated(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == Path)
                    _handler(this, new FileChangedEventArgs(Path, FileChangeType.Created));
            }

            private void OnDeleted(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == Path)
                    _handler(this, new FileChangedEventArgs(Path, FileChangeType.Deleted));
            }

            private void OnRenamed(object sender, RenamedEventArgs e)
            {
                if (e.OldFullPath == Path)
                    _handler(this, new FileChangedEventArgs(Path, FileChangeType.Deleted));
                else if (e.FullPath == Path)
                    _handler(this, new FileChangedEventArgs(Path, FileChangeType.Created));
            }
        }

        private class FolderHandlers : PathHandlersBase
        {
            private readonly FolderChangedEventHandler _handler;

            public FolderHandlers(string path, SharedWatcher sharedWatcher, FolderChangedEventHandler handler)
                : base(path, sharedWatcher)
            {
                _handler = handler;

                FileSystemWatcher fileSystemWatcher = sharedWatcher.FileSystemWatcher;
                fileSystemWatcher.Created += OnCreated;
                fileSystemWatcher.Deleted += OnDeleted;
                fileSystemWatcher.Renamed += OnRenamed;
            }

            protected override void Unsubscribe()
            {
                FileSystemWatcher fileSystemWatcher = SharedWatcher.FileSystemWatcher;
                fileSystemWatcher.Renamed -= OnRenamed;
                fileSystemWatcher.Deleted -= OnDeleted;
                fileSystemWatcher.Created -= OnCreated;
            }

            private void OnCreated(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == Path)
                    _handler(this, new FolderChangedEventArgs(Path, FolderChangeType.Created));
            }

            private void OnDeleted(object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == Path)
                    _handler(this, new FolderChangedEventArgs(Path, FolderChangeType.Deleted));
            }

            private void OnRenamed(object sender, RenamedEventArgs e)
            {
                if (e.OldFullPath == Path)
                    _handler(this, new FolderChangedEventArgs(Path, FolderChangeType.Deleted));
                else if (e.FullPath == Path)
                    _handler(this, new FolderChangedEventArgs(Path, FolderChangeType.Created));
            }
        }
    }
}
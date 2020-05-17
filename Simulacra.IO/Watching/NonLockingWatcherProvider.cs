using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Simulacra.IO.Watching
{
    public class NonLockingWatcherProvider : IFileSystemWatcherProvider<FileSystemWatcher>
    {
        private readonly IWatchableFileSystem _fileSystem;
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watcherByRoot = new ConcurrentDictionary<string, FileSystemWatcher>();

        public NonLockingWatcherProvider(IWatchableFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public virtual FileSystemWatcher GetWatcher(string path)
        {
            // We use a watcher by path root because watching a specific folder will lock all its parent folder with Windows.
            // It probably have an impact on performance but locking folders can have an impact on user experience.

            string pathRoot = _fileSystem.UniqueFolder(Path.GetPathRoot(path));
            if (_watcherByRoot.TryGetValue(pathRoot, out FileSystemWatcher driveWatcher))
                return driveWatcher;

            _watcherByRoot[pathRoot] = driveWatcher = new FileSystemWatcher(pathRoot)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            driveWatcher.FullyReleased += OnWatcherFullyReleased;

            return driveWatcher;
        }

        public bool DisposeWatchersWhenFullyReleased { get; set; } = true;

        private void OnWatcherFullyReleased(object sender, EventArgs e)
        {
            if (!DisposeWatchersWhenFullyReleased)
                return;

            string root = _watcherByRoot.First(x => x.Value == sender).Key;
            _watcherByRoot.TryRemove(root, out FileSystemWatcher watcher);

            watcher.FullyReleased -= OnWatcherFullyReleased;
            watcher.Dispose();
        }

        IFileSystemWatcher IFileSystemWatcherProvider.GetWatcher(string filePath) => GetWatcher(filePath);
    }
}
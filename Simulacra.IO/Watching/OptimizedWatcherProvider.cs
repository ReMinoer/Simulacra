using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Simulacra.IO.Watching
{
    public class OptimizedWatcherProvider : IFileSystemWatcherProvider<FileSystemWatcher>
    {
        private readonly IWatchableFileSystem _fileSystem;
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watcherByFolder = new ConcurrentDictionary<string, FileSystemWatcher>();

        public OptimizedWatcherProvider(IWatchableFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public virtual FileSystemWatcher GetWatcher(string uniquePath)
        {
            string folderPath = _fileSystem.GetFolderPath(uniquePath);
            if (folderPath == null)
                return null;

            string uniqueFolder = _fileSystem.UniqueFolder(folderPath);
            if (_watcherByFolder.TryGetValue(uniqueFolder, out FileSystemWatcher watcher))
                return watcher;

            _watcherByFolder[uniqueFolder] = watcher = new FileSystemWatcher(uniqueFolder)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName
            };

            watcher.FullyReleased += OnWatcherFullyReleased;

            return watcher;
        }

        public bool DisposeWatchersWhenFullyReleased { get; set; } = true;

        private void OnWatcherFullyReleased(object sender, EventArgs e)
        {
            if (!DisposeWatchersWhenFullyReleased)
                return;

            string root = _watcherByFolder.First(x => x.Value == sender).Key;
            _watcherByFolder.TryRemove(root, out FileSystemWatcher watcher);

            watcher.FullyReleased -= OnWatcherFullyReleased;
            watcher.Dispose();
        }

        IFileSystemWatcher IFileSystemWatcherProvider.GetWatcher(string filePath) => GetWatcher(filePath);
    }
}
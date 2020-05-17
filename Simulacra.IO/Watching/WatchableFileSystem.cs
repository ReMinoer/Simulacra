using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class WatchableFileSystem : IWatchableFileSystem<FileSystemWatcher>
    {
        static private WatchableFileSystem _instance;
        static public WatchableFileSystem Instance => _instance ?? (_instance = new WatchableFileSystem());

        private WatchableFileSystem() {}

        public bool PathsCaseSensitive => PathComparer.IsEnvironmentCaseSensitive();
        public bool IsPathRooted(string path) => Path.IsPathRooted(path);
        public bool IsExplicitFolderPath(string path) => PathUtils.IsExplicitFolderPath(path);
        public string GetFolderPath(string path) => PathUtils.GetFolderPath(path);

        public string UniqueFile(string path) => PathUtils.UniqueFile(path);
        public string UniqueFolder(string path) => PathUtils.UniqueFolder(path);

        public bool FileExists(string path) => File.Exists(path);
        public bool FolderExists(string path) => Directory.Exists(path);

        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watcherByRoot = new ConcurrentDictionary<string, FileSystemWatcher>();

        public virtual FileSystemWatcher GetWatcher(string path)
        {
            // We use a watcher by path root because watching a specific folder will lock all its parent folder with Windows.
            // It probably have an impact on performance but locking folders is too much of an impact on user experience.

            string pathRoot = UniqueFolder(Path.GetPathRoot(path));
            if (_watcherByRoot.TryGetValue(pathRoot, out FileSystemWatcher driveWatcher))
                return driveWatcher;

            _watcherByRoot[pathRoot] = driveWatcher =  new FileSystemWatcher(pathRoot)
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

        IFileSystemWatcher IWatchableFileSystem.GetWatcher(string filePath) => GetWatcher(filePath);
    }
}
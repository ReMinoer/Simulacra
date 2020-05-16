using System.IO;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class WatchableFileSystem : IWatchableFileSystem<FileSystemWatcher>
    {
        public bool PathsCaseSensitive => PathComparer.IsEnvironmentCaseSensitive();
        public bool IsPathRooted(string path) => Path.IsPathRooted(path);
        public bool IsExplicitFolderPath(string path) => PathUtils.IsExplicitFolderPath(path);
        public string GetFolderPath(string path) => Path.GetDirectoryName(PathUtils.TrimEndSeparator(path));
        private string GetName(string path) => Path.GetFileName(PathUtils.TrimEndSeparator(path));

        public string UniqueFile(string path) => PathUtils.UniqueFile(path);
        public string UniqueFolder(string path) => PathUtils.UniqueFolder(path);

        public bool FileExists(string path) => File.Exists(path);
        public bool FolderExists(string path) => Directory.Exists(path);

        public virtual FileSystemWatcher GetWatcher(string path)
        {
            string pathRoot = Path.GetPathRoot(path);
            string name = GetName(path);

            return new FileSystemWatcher(pathRoot, name)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };
        }

        IFileSystemWatcher IWatchableFileSystem.GetWatcher(string folderPath) => GetWatcher(folderPath);
    }
}
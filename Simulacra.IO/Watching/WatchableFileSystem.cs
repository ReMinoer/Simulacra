using System.IO;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class WatchableFileSystem : IWatchableFileSystem<FileSystemWatcher>
    {
        public bool IsPathRooted(string path) => Path.IsPathRooted(path);
        public bool IsExplicitFolderPath(string path) => PathUtils.IsExplicitFolderPath(path);
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);

        public string UniqueFile(string path) => PathUtils.UniqueFile(path);
        public string UniqueFolder(string path) => PathUtils.UniqueFolder(path);

        public bool FileExists(string path) => File.Exists(path);
        public bool FolderExists(string path) => Directory.Exists(path);

        public virtual FileSystemWatcher GetWatcher(string folderPath)
        {
            if (!FolderExists(folderPath))
                return null;

            return new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
            };
        }

        IFileSystemWatcher IWatchableFileSystem.GetWatcher(string folderPath) => GetWatcher(folderPath);

    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class WatchableFileSystem : IWatchableFileSystem
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

        public IEnumerable<string> GetFiles(PathPattern pathPattern)
        {
            if (!Directory.Exists(pathPattern.FolderPath))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(pathPattern.FolderPath, pathPattern.NamePattern);
        }

        public IEnumerable<string> GetFolders(PathPattern pathPattern)
        {
            if (!Directory.Exists(pathPattern.FolderPath))
                return Enumerable.Empty<string>();

            return Directory.GetDirectories(pathPattern.FolderPath, pathPattern.NamePattern);
        }
    }
}
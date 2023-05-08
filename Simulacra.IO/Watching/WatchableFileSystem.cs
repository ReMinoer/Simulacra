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

        static private PathSystem _pathSystem;
        static private PathSystem PathSystem => _pathSystem ?? (_pathSystem = PathSystem.Instance);

        private WatchableFileSystem() {}

        public bool PathsCaseSensitive => PathSystem.PathsCaseSensitive;
        public char[] Separators => PathSystem.Separators;
        public char AbsoluteSeparator => PathSystem.AbsoluteSeparator;
        public char RelativeSeparator => PathSystem.RelativeSeparator;
        public char[] InvalidPathChars => PathSystem.InvalidPathChars;
        public bool IsPathRooted(string path) => PathSystem.IsPathRooted(path);

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
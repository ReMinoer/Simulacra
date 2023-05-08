using System;
using System.IO;

namespace Simulacra.IO
{
    public class PathSystem : IPathSystem
    {
        static private PathSystem _instance;
        static public PathSystem Instance => _instance ?? (_instance = new PathSystem());
        
        private PathSystem() {}

        public bool PathsCaseSensitive => Environment.OSVersion.Platform == PlatformID.Unix;
        public char[] Separators { get; } = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        public char AbsoluteSeparator => Path.DirectorySeparatorChar;
        public char RelativeSeparator => Path.AltDirectorySeparatorChar;
        public char[] InvalidPathChars => Path.GetInvalidPathChars();
        public bool IsPathRooted(string path) => Path.IsPathRooted(path);
    }
}
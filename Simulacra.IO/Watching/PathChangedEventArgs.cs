using System;
using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class PathChangedEventArgs : EventArgs
    {
        public PathPattern WatchedPathPattern { get; }
        public string Path { get; }
        public string NewPath { get; }
        public string OldPath { get; }

        public PathChangedEventArgs(PathPattern watchedPathPattern, string path, string newPath = null, string oldPath = null)
        {
            WatchedPathPattern = watchedPathPattern;
            Path = path;
            NewPath = newPath;
            OldPath = oldPath;
        }
    }
}
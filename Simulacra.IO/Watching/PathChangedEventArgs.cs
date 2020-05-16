using System;

namespace Simulacra.IO.Watching
{
    public class PathChangedEventArgs : EventArgs
    {
        public string Path { get; }
        public string NewPath { get; }
        public string OldPath { get; }

        public PathChangedEventArgs(string path, string newPath = null, string oldPath = null)
        {
            Path = path;
            NewPath = newPath;
            OldPath = oldPath;
        }
    }
}
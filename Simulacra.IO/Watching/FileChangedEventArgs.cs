using System;

namespace Simulacra.IO.Watching
{
    public class FileChangedEventArgs : EventArgs
    {
        public string Path { get; }
        public FileChangeType ChangeType { get; }

        public FileChangedEventArgs(string path, FileChangeType changeType)
        {
            Path = path;
            ChangeType = changeType;
        }
    }
}
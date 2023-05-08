using System;

namespace Simulacra.IO.Watching
{
    public class FileSystemChangedEventArgs : EventArgs
    {
        public FileSystemChangeType ChangeType { get; }
        public string FullPath { get; }

        public FileSystemChangedEventArgs(FileSystemChangeType changeType, string fullPath)
        {
            ChangeType = changeType;
            FullPath = fullPath;
        }
    }
}
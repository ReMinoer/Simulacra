using System;

namespace Simulacra.IO.Watching
{
    public class FolderChangedEventArgs : EventArgs
    {
        public string Path { get; }
        public FolderChangeType ChangeType { get; }

        public FolderChangedEventArgs(string path, FolderChangeType changeType)
        {
            Path = path;
            ChangeType = changeType;
        }
    }
}
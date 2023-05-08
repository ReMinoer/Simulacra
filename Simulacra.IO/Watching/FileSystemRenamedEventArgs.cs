namespace Simulacra.IO.Watching
{
    public class FileSystemRenamedEventArgs : FileSystemChangedEventArgs
    {
        public string OldFullPath { get; }

        public FileSystemRenamedEventArgs(string oldFullPath, string newFullPath)
            : base(FileSystemChangeType.Renamed, newFullPath)
        {
            OldFullPath = oldFullPath;
        }
    }
}
namespace Simulacra.IO.Watching
{
    public class FolderChangedEventArgs : PathChangedEventArgs
    {
        public FolderChangeType ChangeType { get; }

        public FolderChangedEventArgs(string path, FolderChangeType changeType, string newPath = null, string oldPath = null)
            : base(path, newPath, oldPath)
        {
            ChangeType = changeType;
        }
    }
}
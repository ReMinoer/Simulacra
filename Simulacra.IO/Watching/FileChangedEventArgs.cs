namespace Simulacra.IO.Watching
{
    public class FileChangedEventArgs : PathChangedEventArgs
    {
        public FileChangeType ChangeType { get; }

        public FileChangedEventArgs(string path, FileChangeType changeType, string newPath = null, string oldPath = null)
            : base(path, newPath, oldPath)
        {
            ChangeType = changeType;
        }
    }
}
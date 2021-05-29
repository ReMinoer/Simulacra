using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class FolderChangedEventArgs : PathChangedEventArgs
    {
        public FolderChangeType ChangeType { get; }

        public FolderChangedEventArgs(PathPattern watchedPathPattern, string path, FolderChangeType changeType, string newPath = null, string oldPath = null)
            : base(watchedPathPattern, path, newPath, oldPath)
        {
            ChangeType = changeType;
        }
    }
}
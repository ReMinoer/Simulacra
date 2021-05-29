using Simulacra.IO.Utils;

namespace Simulacra.IO.Watching
{
    public class FileChangedEventArgs : PathChangedEventArgs
    {
        public FileChangeType ChangeType { get; }

        public FileChangedEventArgs(PathPattern watchedPathPattern, string path, FileChangeType changeType, string newPath = null, string oldPath = null)
            : base(watchedPathPattern, path, newPath, oldPath)
        {
            ChangeType = changeType;
        }
    }
}
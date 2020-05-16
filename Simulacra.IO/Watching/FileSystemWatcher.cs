namespace Simulacra.IO.Watching
{
    public class FileSystemWatcher : System.IO.FileSystemWatcher, IFileSystemWatcher
    {
        public FileSystemWatcher()
        {
        }

        public FileSystemWatcher(string path)
            : base(path)
        {
        }

        public FileSystemWatcher(string path, string filter)
            : base(path, filter)
        {
        }
    }
}
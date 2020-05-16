using System.IO;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test.Mocking
{
    public class MockFileSystemWatcher : IFileSystemWatcher
    {
        private readonly string _folderPath;

        public bool EnableRaisingEvents { private get; set; }
        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event RenamedEventHandler Renamed;

        public MockFileSystemWatcher(string folderPath)
        {
            _folderPath = folderPath;
        }

        public void Change(string name) => Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, _folderPath, name));
        public void Create(string name) => Created?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, _folderPath, name));
        public void Delete(string name) => Deleted?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _folderPath, name));
        public void Rename(string oldName, string newName) => Renamed?.Invoke(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, _folderPath, newName, oldName));

        public void Dispose() { }
    }
}
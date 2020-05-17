using System;
using System.IO;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test.Mocking
{
    public class MockFileSystemWatcher : IFileSystemWatcher
    {
        private readonly string _folderPath;
        private readonly string _name;
        private int _counter;

        public event FileSystemEventHandler Changed;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Deleted;
        public event RenamedEventHandler Renamed;
        public event EventHandler FullyReleased;

        public MockFileSystemWatcher(string folderPath, string name)
        {
            _folderPath = folderPath;
            _name = name;
        }

        public void Change() => Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, _folderPath, _name));
        public void Create() => Created?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, _folderPath, _name));
        public void Delete() => Deleted?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _folderPath, _name));
        public void RenameTo(string newName) => Renamed?.Invoke(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, _folderPath, newName, _name));
        public void RenameFrom(string oldName) => Renamed?.Invoke(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, _folderPath, _name, oldName));

        public void Increment() => _counter++;
        public void Enable() {}
        public void Release()
        {
            _counter--;
            if (_counter > 0)
                return;

            FullyReleased?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() {}
    }
}
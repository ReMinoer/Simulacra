using System;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test.Mocking
{
    public class MockFileSystemWatcher : IFileSystemWatcher
    {
        private readonly string _folderPath;
        private readonly string _name;
        private readonly IPathSystem _pathSystem;
        private int _counter;

        public event FileSystemChangedEventHandler Changed;
        public event FileSystemChangedEventHandler Created;
        public event FileSystemChangedEventHandler Deleted;
        public event FileSystemRenamedEventHandler Renamed;
        public event EventHandler FullyReleased;

        public MockFileSystemWatcher(string folderPath, string name, IPathSystem pathSystem)
        {
            _folderPath = folderPath;
            _name = name;
            _pathSystem = pathSystem;
        }

        public void Change(string path) => Changed?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Changed, path));
        public void Create(string path) => Created?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Created, path));
        public void Delete(string path) => Deleted?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Deleted, path));
        public void Rename(string path, string newName)
        {
            string newPath = _pathSystem.Combine(_pathSystem.GetFolderPath(path), newName);
            Renamed?.Invoke(this, new FileSystemRenamedEventArgs(path, newPath));
        }

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
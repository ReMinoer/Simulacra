using System;
using System.IO;

namespace Simulacra.IO.Watching
{
    public class FileSystemWatcher : System.IO.FileSystemWatcher, IFileSystemWatcher
    {
        private int _counter;

        new public event FileSystemChangedEventHandler Changed;
        new public event FileSystemChangedEventHandler Created;
        new public event FileSystemChangedEventHandler Deleted;
        new public event FileSystemRenamedEventHandler Renamed;
        public event EventHandler FullyReleased;

        public FileSystemWatcher()
        {
            SubscribeToBaseEvents();
        }

        public FileSystemWatcher(string folderPath)
            : base(folderPath)
        {
            SubscribeToBaseEvents();
        }

        public FileSystemWatcher(string folderPath, string filter)
            : base(folderPath, filter)
        {
            SubscribeToBaseEvents();
        }

        private void SubscribeToBaseEvents()
        {
            base.Changed += OnChanged;
            base.Created += OnCreated;
            base.Deleted += OnDeleted;
            base.Renamed += OnRenamed;
        }

        private void OnChanged(object sender, FileSystemEventArgs e) => Changed?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Changed, e.FullPath));
        private void OnCreated(object sender, FileSystemEventArgs e) => Created?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Created, e.FullPath));
        private void OnDeleted(object sender, FileSystemEventArgs e) => Deleted?.Invoke(this, new FileSystemChangedEventArgs(FileSystemChangeType.Deleted, e.FullPath));
        private void OnRenamed(object sender, RenamedEventArgs e) => Renamed?.Invoke(this, new FileSystemRenamedEventArgs(e.OldFullPath, e.Name));
        
        void IFileSystemWatcher.Enable() => EnableRaisingEvents = true;

        public void Increment() => _counter++;
        public void Release()
        {
            _counter--;
            if (_counter > 0)
                return;

            EnableRaisingEvents = false;
            FullyReleased?.Invoke(this, EventArgs.Empty);
        }
    }
}
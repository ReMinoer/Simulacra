using System;

namespace Simulacra.IO.Watching
{
    public class FileSystemWatcher : System.IO.FileSystemWatcher, IFileSystemWatcher
    {
        private int _counter;
        public event EventHandler FullyReleased;

        public FileSystemWatcher()
        {
        }

        public FileSystemWatcher(string folderPath)
            : base(folderPath)
        {
        }

        public FileSystemWatcher(string folderPath, string filter)
            : base(folderPath, filter)
        {
        }

        public void Increment() => _counter++;
        void IFileSystemWatcher.Enable() => EnableRaisingEvents = true;
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
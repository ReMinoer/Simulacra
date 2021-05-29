﻿using System;
using System.IO;
using Simulacra.IO.Utils;
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

        public void Change(string path) => Changed?.Invoke(this, GetEventArgs(WatcherChangeTypes.Changed, path));
        public void Create(string path) => Created?.Invoke(this, GetEventArgs(WatcherChangeTypes.Created, path));
        public void Delete(string path) => Deleted?.Invoke(this, GetEventArgs(WatcherChangeTypes.Deleted, path));
        public void Rename(string path, string newName) => Renamed?.Invoke(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, PathUtils.GetFolderPath(path), newName, PathUtils.GetName(path)));

        private FileSystemEventArgs GetEventArgs(WatcherChangeTypes changeType, string path)
        {
            return new FileSystemEventArgs(changeType, PathUtils.GetFolderPath(path), PathUtils.GetName(path));
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
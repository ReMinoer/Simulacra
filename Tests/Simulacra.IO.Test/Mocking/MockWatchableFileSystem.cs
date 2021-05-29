﻿using System;
using System.Collections.Generic;
using System.Linq;
using Simulacra.IO.Utils;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test.Mocking
{
    public class MockWatchableFileSystem : IWatchableFileSystem
    {
        private const char AbsoluteSeparator = '\\';

        private List<Action> _batch;
        private readonly HashSet<string> _existingPaths = new HashSet<string>();

        public bool PathsCaseSensitive => false;
        public bool IsPathRooted(string path) => path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':';
        public bool IsExplicitFolderPath(string path) => path.EndsWith(AbsoluteSeparator);

        public MockFileSystemWatcherProvider WatcherProvider { get; }

        public MockWatchableFileSystem()
        {
            WatcherProvider = new MockFileSystemWatcherProvider(this);
        }

        static private string Combine(string left, string right)
        {
            if (!left.EndsWith(AbsoluteSeparator))
                left += AbsoluteSeparator;
            return left + right;
        }

        public string GetFolderPath(string path)
        {
            path = path.TrimEnd(AbsoluteSeparator);

            int lastSeparator = path.LastIndexOf(AbsoluteSeparator);
            if (lastSeparator == -1)
                return null;

            return path.Substring(0, lastSeparator);
        }

        static public string GetName(string path)
        {
            path = path.TrimEnd(AbsoluteSeparator);

            int lastSeparator = path.LastIndexOf(AbsoluteSeparator);
            if (lastSeparator == -1 || lastSeparator > path.Length - 1)
                return null;

            return path.Substring(lastSeparator + 1, path.Length - lastSeparator - 1);
        }

        public string UniqueFile(string path)
        {
            if (IsExplicitFolderPath(path))
                throw new ArgumentException();

            return path.ToLowerInvariant();
        }

        public string UniqueFolder(string path)
        {
            path = path.ToLowerInvariant();

            if (!IsExplicitFolderPath(path))
                path += AbsoluteSeparator;

            return path;
        }

        public bool FileExists(string filePath) => _existingPaths.Contains(UniqueFile(filePath));
        public bool FolderExists(string folderPath)
        {
            folderPath = UniqueFolder(folderPath);
            return _existingPaths.Any(x => x.StartsWith(folderPath));
        }

        public IEnumerable<string> GetFiles(PathPattern pathPattern) => _existingPaths.Where(x => !PathUtils.IsExplicitFolderPath(x)).Where(pathPattern.Match);
        public IEnumerable<string> GetFolders(PathPattern pathPattern) => _existingPaths.Where(PathUtils.IsExplicitFolderPath).Where(pathPattern.Match);

        private void AddPath(string path)
        {
            _existingPaths.Add(path);

            while (true)
            {
                string folderPath = GetFolderPath(path);
                if (folderPath == null)
                    return;

                string uniquePath = UniqueFolder(folderPath);
                if (_existingPaths.Add(uniquePath))
                {
                    foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(uniquePath))
                        watcher.Create(path);
                }

                path = folderPath;
            }
        }

        private void RemovePath(string path)
        {
            string[] pathsToRemoveArray = _existingPaths.Where(x => x.StartsWith(path)).ToArray();
            foreach (string pathToRemove in pathsToRemoveArray)
                _existingPaths.Remove(pathToRemove);
        }

        public void StartBatching()
        {
            _batch = new List<Action>();
        }

        public void StopBatching()
        {
            foreach (Action action in _batch)
                action();

            _batch = null;
        }

        public void Change(string path)
        {
            if (!FileExists(path))
                throw new InvalidOperationException();

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent()
            {
                foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(path))
                    watcher.Change(path);
            }
        }

        public void Create(string uniquePath)
        {
            AddPath(uniquePath);

            if (!IsExplicitFolderPath(uniquePath))
            {
                string folderPath = GetFolderPath(uniquePath);
                if (folderPath == null || !FolderExists(UniqueFolder(folderPath)))
                    throw new InvalidOperationException();
            }

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent()
            {
                foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(uniquePath))
                    watcher.Create(uniquePath);
            }
        }

        public void Delete(string uniquePath)
        {
            RemovePath(uniquePath);

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent()
            {
                foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(uniquePath))
                    watcher.Delete(uniquePath);
            }
        }

        public void Rename(string uniqueOldPath, string uniqueNewName)
        {
            string folderPath = GetFolderPath(uniqueOldPath);
            if (folderPath == null)
                return;

            string uniqueNewPath = Combine(folderPath, uniqueNewName);

            RemovePath(uniqueOldPath);
            AddPath(uniqueNewPath);

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent()
            {
                foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(uniqueOldPath))
                    watcher.Rename(uniqueOldPath, uniqueNewName);
                foreach (MockFileSystemWatcher watcher in WatcherProvider.GetAllMatchingWatchers(uniqueNewPath))
                    watcher.Rename(uniqueOldPath, uniqueNewName);
            }
        }

        public void Reset()
        {
            WatcherProvider.Reset();
            _existingPaths.Clear();
        }

        public class MockFileSystemWatcherProvider : IFileSystemWatcherProvider<MockFileSystemWatcher>
        {
            private readonly MockWatchableFileSystem _fileSystem;
            private readonly Dictionary<string, MockFileSystemWatcher> _watchers = new Dictionary<string, MockFileSystemWatcher>();

            public MockFileSystemWatcherProvider(MockWatchableFileSystem fileSystem)
            {
                _fileSystem = fileSystem;
            }

            public IEnumerable<MockFileSystemWatcher> GetAllMatchingWatchers(string uniquePath)
            {
                GetWatcher(uniquePath);
                return _watchers.Where(x => PathPattern.Match(uniquePath, x.Key, caseSensitive: true)).Select(x => x.Value).ToArray();
            }

            IFileSystemWatcher IFileSystemWatcherProvider.GetWatcher(string folderPath) => GetWatcher(folderPath);
            public MockFileSystemWatcher GetWatcher(string uniquePath)
            {
                if (_watchers.TryGetValue(uniquePath, out MockFileSystemWatcher watcher))
                    return watcher;

                string folderPath = _fileSystem.GetFolderPath(uniquePath);
                if (folderPath == null)
                    return null;

                string name = GetName(uniquePath);
                _watchers[uniquePath] = watcher = new MockFileSystemWatcher(folderPath, name);
                watcher.FullyReleased += OnWatcherFullyReleased;

                return watcher;
            }

            private void OnWatcherFullyReleased(object sender, EventArgs e)
            {
                KeyValuePair<string, MockFileSystemWatcher> pair = _watchers.First(x => x.Value == sender);
                string path = pair.Key;
                MockFileSystemWatcher watcher = pair.Value;

                _watchers.Remove(path);

                watcher.FullyReleased -= OnWatcherFullyReleased;
                watcher.Dispose();
            }

            public void Reset()
            {
                _watchers.Clear();
            }
        }
    }
}
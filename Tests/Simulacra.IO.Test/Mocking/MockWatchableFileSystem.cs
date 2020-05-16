using System;
using System.Collections.Generic;
using System.Linq;
using Simulacra.IO.Utils;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test.Mocking
{
    public class MockWatchableFileSystem : IWatchableFileSystem<MockFileSystemWatcher>
    {
        private const char AbsoluteSeparator = '\\';

        private List<Action> _batch;
        private readonly Dictionary<string, MockFileSystemWatcher> _watchers = new Dictionary<string, MockFileSystemWatcher>();
        private readonly HashSet<string> _existingPaths = new HashSet<string>();

        public bool IsPathRooted(string path) => path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':';
        public bool IsExplicitFolderPath(string path) => path.EndsWith(AbsoluteSeparator);

        private string Combine(string left, string right)
        {
            if (!left.EndsWith(AbsoluteSeparator))
                left += AbsoluteSeparator;
            return left + right;
        }

        public string GetDirectoryName(string path)
        {
            path = path.TrimEnd(AbsoluteSeparator);

            int lastSeparator = path.LastIndexOf(AbsoluteSeparator);
            if (lastSeparator == -1)
                return null;

            return path.Substring(0, lastSeparator);
        }

        private string GetName(string path)
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

        public bool FileExists(string path) => _existingPaths.Contains(path);
        public bool FolderExists(string path) => _existingPaths.Any(x => x.StartsWith(path));

        private void AddPath(string path)
        {
            _existingPaths.Add(path);

            //string uniquePath = path;
            string name = GetName(path);
            bool createPreviousPath = false;

            while (true)
            {
                string folderPath = GetDirectoryName(path);
                if (folderPath == null)
                    return;

                if (createPreviousPath)
                {
                    GetWatcher(folderPath).Create(name);
                    createPreviousPath = false;
                }

                if (_existingPaths.Add(UniqueFolder(folderPath)))
                    createPreviousPath = true;

                path = folderPath;
                //uniquePath = UniqueFolder(path);
                name = GetName(path);
            }
        }

        private void RemovePath(string path)
        {
            //string folderPath = UniqueFolder(GetDirectoryName(PathComparer.TransformFolder(path, FolderPathEquality.RespectAmbiguity)));

            string[] pathsToRemoveArray = _existingPaths.Where(x => x.StartsWith(path) && x != path).ToArray();
            foreach (string pathToRemove in pathsToRemoveArray)
            {
                string p = PathComparer.TransformFolder(pathToRemove, FolderPathEquality.RespectAmbiguity);
                string folderPath = GetDirectoryName(p);
                if (folderPath == null)
                    return;

                MockFileSystemWatcher watcher = GetWatcher(folderPath);
                string name = GetName(p);

                _existingPaths.Remove(pathToRemove);
                watcher?.Delete(name);
            }

            _existingPaths.Remove(path);
        }

        public MockFileSystemWatcher GetWatcher(string folderPath)
        {
            folderPath = UniqueFolder(folderPath);
            if (!_existingPaths.Contains(folderPath))
                return null;

            if (!_watchers.TryGetValue(folderPath, out MockFileSystemWatcher watcher))
                _watchers[folderPath] = watcher = new MockFileSystemWatcher(folderPath);
            return watcher;
        }

        IFileSystemWatcher IWatchableFileSystem.GetWatcher(string folderPath) => GetWatcher(folderPath);

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

            string folderPath = GetDirectoryName(path);
            if (folderPath == null)
                return;

            string name = GetName(path);

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent() => GetWatcher(folderPath).Change(name);
        }

        public void Create(string path)
        {
            AddPath(path);
            string folderPath = GetDirectoryName(path);

            if (!IsExplicitFolderPath(path))
            {
                if (folderPath == null || !FolderExists(UniqueFolder(folderPath)))
                    throw new InvalidOperationException();
            }

            if (folderPath == null)
                return;

            string name = GetName(path);

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent() => GetWatcher(folderPath).Create(name);
        }

        public void Delete(string path)
        {
            RemovePath(path);

            string folderPath = GetDirectoryName(path);
            if (folderPath == null)
                return;

            string name = GetName(path);

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent() => GetWatcher(folderPath).Delete(name);
        }

        public void Rename(string oldPath, string newName)
        {
            string folderPath = GetDirectoryName(oldPath);
            if (folderPath == null)
                return;

            string oldName = GetName(oldPath);

            RemovePath(oldPath);
            AddPath(Combine(folderPath, newName));

            if (_batch != null)
            {
                _batch.Add(TriggerEvent);
                return;
            }

            TriggerEvent();
            void TriggerEvent() => GetWatcher(folderPath).Rename(oldName, newName);
        }
    }
}
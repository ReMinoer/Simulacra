using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Simulacra.IO.Test.Mocking;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test
{
    public class FileFolderWatcherTest
    {
        private const string FilePath = @"c:\folder\file.ext";
        private const string FolderPath = @"c:\folder\subfolder\";

        [TestCase(FilePath)]
        public void ChangeFile(string path)
        {
            TestFile(x => PreparePath(x, path), x => x.Change(path), Expect(path, FileChangeType.Edited));
        }

        [TestCase(FilePath)]
        public void CreateFile(string path)
        {
            TestFile(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(path, FileChangeType.Created));
        }

        [TestCase(FilePath)]
        public void DeleteFile(string path)
        {
            TestFile(x => PreparePath(x, path), x => x.Delete(path), Expect(path, FileChangeType.Deleted));
        }

        [TestCase(@"c:\folder\", "file.old", "file.new")]
        public void RenameFile(string folder, string oldName, string newName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;
            TestFile(x => PreparePath(x, oldPath), x => x.Rename(oldPath, newName), Expect(oldPath, FileChangeType.Deleted), Expect(newPath, FileChangeType.Created));
        }

        [TestCase(FilePath)]
        public void QuicklyCreateDeleteFile(string path)
        {
            TestFile(x => PrepareParentFolder(x, path), x =>
            {
                x.StartBatching();
                x.Create(path);
                x.Delete(path);
                x.StopBatching();
            }, Expect(path, FileChangeType.Created, FileChangeType.Deleted));
        }

        [TestCase(FilePath)]
        public void QuicklyDeleteCreateFile(string path)
        {
            TestFile(x => PreparePath(x, path), x =>
            {
                x.StartBatching();
                x.Delete(path);
                x.Create(path);
                x.StopBatching();
            }, Expect(path, FileChangeType.Deleted, FileChangeType.Created));
        }

        [TestCase(FolderPath)]
        public void CreateFolder(string path)
        {
            TestFolder(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(path, FolderChangeType.Created));
        }

        [TestCase(FolderPath)]
        public void DeleteFolder(string path)
        {
            TestFolder(x => PreparePath(x, path), x => x.Delete(path), Expect(path, FolderChangeType.Deleted));
        }

        [TestCase(@"c:\folder\", @"oldfolder\", @"newfolder\")]
        public void RenameFolder(string folder, string oldName, string newName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;
            TestFolder(x => PreparePath(x, oldPath), x => x.Rename(oldPath, newName), Expect(oldPath, FolderChangeType.Deleted), Expect(newPath, FolderChangeType.Created));
        }

        [TestCase(FolderPath)]
        public void QuicklyCreateDeleteFolder(string path)
        {
            TestFolder(x => PrepareParentFolder(x, path), x =>
            {
                x.StartBatching();
                x.Create(path);
                x.Delete(path);
                x.StopBatching();
            }, Expect(path, FolderChangeType.Created, FolderChangeType.Deleted));
        }

        [TestCase(FolderPath)]
        public void QuicklyDeleteCreateFolder(string path)
        {
            TestFolder(x => PreparePath(x, path), x =>
            {
                x.StartBatching();
                x.Delete(path);
                x.Create(path);
                x.StopBatching();
            }, Expect(path, FolderChangeType.Deleted, FolderChangeType.Created));
        }

        [TestCase(@"c:\folder\", "file.ext")]
        public void WatchFileWithMissingFolder(string folder, string name)
        {
            string path = folder + name;
            TestFile(x => PrepareRootFolder(x, path), x =>
            {
                x.Create(folder);
                x.Create(path);
                x.Change(path);
            }, Expect(path, FileChangeType.Created, FileChangeType.Edited));
        }

        [TestCase(@"c:\folder\", "file.ext")]
        public void RemoveFolderContainingWatchedFile(string folder, string name)
        {
            string path = folder + name;
            TestFile(x => PreparePath(x, path), x => x.Delete(folder), Expect(path, FileChangeType.Deleted));
        }

        private void TestFile(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params (string, FileChangeType[])[] expectedChanges)
        {
            var mockSystem = new MockWatchableFileSystem();
            prepareSystem?.Invoke(mockSystem);

            var watcher = new FileFolderWatcher(mockSystem)
            {
                Logger = new ConsoleLogger()
            };

            var handled = new bool[expectedChanges.Length];
            (string, FileChangedEventHandler)[] handlers = expectedChanges.Select((x, i) => (x.Item1, GetFileHandler(watcher, x.Item1, x.Item2, () => handled[i] = true))).ToArray();

            foreach ((string path, FileChangedEventHandler handler) in handlers)
                watcher.WatchFile(path, handler);
            {
                handled.Should().AllBeEquivalentTo(false);
            }

            mockAction(mockSystem);
            {
                handled.Should().AllBeEquivalentTo(true);
            }

            for (int i = 0; i < handled.Length; i++)
                handled[i] = false;

            foreach ((string _, FileChangedEventHandler handler) in handlers)
                watcher.Unwatch(handler);
            {
                handled.Should().AllBeEquivalentTo(false);
            }

            mockAction(mockSystem);
            {
                handled.Should().AllBeEquivalentTo(false);
            }
        }

        private void TestFolder(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params (string, FolderChangeType[])[] expectedChanges)
        {
            var mockSystem = new MockWatchableFileSystem();
            prepareSystem?.Invoke(mockSystem);

            var watcher = new FileFolderWatcher(mockSystem)
            {
                Logger = new ConsoleLogger()
            };

            var handled = new bool[expectedChanges.Length];
            (string, FolderChangedEventHandler)[] handlers = expectedChanges.Select((x, i) => (x.Item1, GetFolderHandler(watcher, x.Item1, x.Item2, () => handled[i] = true))).ToArray();

            foreach ((string path, FolderChangedEventHandler handler) in handlers)
                watcher.WatchFolder(path, handler);
            {
                handled.Should().AllBeEquivalentTo(false);
            }

            mockAction(mockSystem);
            {
                handled.Should().AllBeEquivalentTo(true);
            }

            for (int i = 0; i < handled.Length; i++)
                handled[i] = false;

            foreach ((string _, FolderChangedEventHandler handler) in handlers)
                watcher.Unwatch(handler);
            {
                handled.Should().AllBeEquivalentTo(false);
            }

            mockAction(mockSystem);
            {
                handled.Should().AllBeEquivalentTo(false);
            }
        }

        private FileChangedEventHandler GetFileHandler(FileFolderWatcher watcher, string fullPath, FileChangeType[] changeTypes, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                sender.Should().Be(watcher);
                args.Path.Should().Be(fullPath);
                args.ChangeType.Should().Be(changeTypes[i]);
                action();
                i++;
            };
        }

        private FolderChangedEventHandler GetFolderHandler(FileFolderWatcher watcher, string fullPath, FolderChangeType[] changeTypes, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                sender.Should().Be(watcher);
                args.Path.Should().Be(fullPath);
                args.ChangeType.Should().Be(changeTypes[i]);
                action();
                i++;
            };
        }

        private (string, T[]) Expect<T>(string path, params T[] changeTypes) => (path, changeTypes);

        private void PrepareRootFolder(MockWatchableFileSystem fileSystem, string path)
        {
            string rootFolder = path;
            while (fileSystem.GetDirectoryName(rootFolder) != null)
                rootFolder = fileSystem.GetDirectoryName(rootFolder);

            fileSystem.Create(fileSystem.UniqueFolder(rootFolder));
        }

        private void PrepareParentFolder(MockWatchableFileSystem fileSystem, string path)
        {
            fileSystem.Create(fileSystem.UniqueFolder(fileSystem.GetDirectoryName(path)));
        }

        private void PreparePath(MockWatchableFileSystem fileSystem, string path)
        {
            PrepareParentFolder(fileSystem, path);
            fileSystem.Create(path);
        }

        private class ConsoleLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine($"[{logLevel.ToString().ToUpperInvariant()}] {state}");
            }

            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }
}
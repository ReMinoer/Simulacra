using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Simulacra.IO.Test.Mocking;
using Simulacra.IO.Utils;
using Simulacra.IO.Watching;

namespace Simulacra.IO.Test
{
    public class PathWatcherTest
    {
        private const string FilePath = @"c:\folder\file.ext";
        private const string FolderPath = @"c:\folder\subfolder\";

        [TestCase(FilePath)]
        public void ChangeFile(string path)
        {
            TestFile(x => PreparePath(x, path), x => x.Change(path), Expect(path, null, null, FileChangeType.Edited));
        }

        [TestCase(FilePath)]
        public void CreateFile(string path)
        {
            TestFile(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(path, null, null, FileChangeType.Created));
        }

        [TestCase(FilePath)]
        public void DeleteFile(string path)
        {
            TestFile(x => PreparePath(x, path), x => x.Delete(path), Expect(path, null, null, FileChangeType.Deleted));
        }

        [TestCase(@"c:\folder\", "file.old", "file.new")]
        public void RenameFile(string folder, string oldName, string newName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;

            TestFile(x => PreparePath(x, oldPath),
                x => x.Rename(oldPath, newName),
                Expect(oldPath, newPath, null, FileChangeType.Deleted),
                Expect(newPath, null, oldPath, FileChangeType.Created));
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
            }, Expect(path, null, null, FileChangeType.Created, FileChangeType.Deleted));
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
            }, Expect(path, null, null, FileChangeType.Deleted, FileChangeType.Created));
        }

        [TestCase(FolderPath)]
        public void CreateFolder(string path)
        {
            TestFolder(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(path, null, null, FolderChangeType.Created));
        }

        [TestCase(FolderPath)]
        public void DeleteFolder(string path)
        {
            TestFolder(x => PreparePath(x, path), x => x.Delete(path), Expect(path, null, null, FolderChangeType.Deleted));
        }

        [TestCase(@"c:\folder\", @"oldfolder\", @"newfolder\")]
        public void RenameFolder(string folder, string oldName, string newName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;

            TestFolder(x => PreparePath(x, oldPath),
                x => x.Rename(oldPath, newName),
                Expect(oldPath, newPath, null, FolderChangeType.Deleted),
                Expect(newPath, null, oldPath, FolderChangeType.Created));
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
            }, Expect(path, null, null, FolderChangeType.Created, FolderChangeType.Deleted));
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
            }, Expect(path, null, null, FolderChangeType.Deleted, FolderChangeType.Created));
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
            }, Expect(path, null, null, FileChangeType.Created, FileChangeType.Edited));
        }

        [TestCase(@"c:\folder\", "file.ext")]
        public void RemoveFolderContainingWatchedFile(string folder, string name)
        {
            string path = folder + name;
            TestFile(x => PreparePath(x, path), x => x.Delete(folder), Expect(path, null, null, FileChangeType.Deleted));
        }

        private void TestFile(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params (string, string, string, FileChangeType[])[] expectedChanges)
            => TestBase(
                prepareSystem, mockAction, expectedChanges, GetFileHandler,
                (watcher, path, handler) => watcher.WatchFile(path, handler),
                (watcher, path, handler) => watcher.Unwatch(path, handler)
            );

        private void TestFolder(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params (string, string, string, FolderChangeType[])[] expectedChanges)
            => TestBase(
                prepareSystem, mockAction, expectedChanges, GetFolderHandler,
                (watcher, path, handler) => watcher.WatchFolder(path, handler),
                (watcher, path, handler) => watcher.Unwatch(path, handler)
            );

        private void TestBase<TChangeType, THandler>(
            Action<MockWatchableFileSystem> prepareSystem,
            Action<MockWatchableFileSystem> mockAction,
            (string, string, string, TChangeType[])[] expectedChanges,
            Func<PathWatcher, string, TChangeType[], string, string, Action, THandler> getHandlerFunc,
            Action<PathWatcher, string, THandler> watchAction,
            Action<PathWatcher, string, THandler> unwatchAction)
        {
            var mockSystem = new MockWatchableFileSystem();

            var watcher = new PathWatcher(mockSystem, mockSystem.WatcherProvider)
            {
                Logger = new ConsoleLogger()
            };

            Console.WriteLine("--- First pass ---");
            TestPass();
            Console.WriteLine("--- Second pass ---");
            TestPass();

            void TestPass()
            {
                prepareSystem?.Invoke(mockSystem);

                var handled = new bool[expectedChanges.Length];
                (string, THandler)[] handlers = expectedChanges.Select((x, i) => (x.Item1, getHandlerFunc(watcher, x.Item1, x.Item4, x.Item2, x.Item3, () => handled[i] = true))).ToArray();

                foreach ((string path, THandler handler) in handlers)
                    watchAction(watcher, path, handler);
                {
                    handled.Should().AllBeEquivalentTo(false);
                }

                mockAction(mockSystem);
                {
                    handled.Should().AllBeEquivalentTo(true);
                }

                for (int i = 0; i < handled.Length; i++)
                    handled[i] = false;

                foreach ((string path, THandler handler) in handlers)
                    unwatchAction(watcher, path, handler);
                {
                    handled.Should().AllBeEquivalentTo(false);
                }

                mockAction(mockSystem);
                {
                    handled.Should().AllBeEquivalentTo(false);
                }

                mockSystem.Reset();
            }
        }

        private FileChangedEventHandler GetFileHandler(PathWatcher watcher, string fullPath, FileChangeType[] changeTypes, string newPath, string oldPath, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                sender.Should().Be(watcher);
                args.Path.Should().Match(x => PathEquals(x, fullPath));
                args.ChangeType.Should().Be(changeTypes[i]);
                args.NewPath.Should().Match(x => PathEquals(x, newPath));
                args.OldPath.Should().Match(x => PathEquals(x, oldPath));
                action();
                i++;
            };
        }

        private FolderChangedEventHandler GetFolderHandler(PathWatcher watcher, string fullPath, FolderChangeType[] changeTypes, string newPath, string oldPath, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                sender.Should().Be(watcher);
                args.Path.Should().Match(x => PathEquals(x, fullPath));
                args.ChangeType.Should().Be(changeTypes[i]);
                args.NewPath.Should().Match(x => PathEquals(x, newPath));
                args.OldPath.Should().Match(x => PathEquals(x, oldPath));
                action();
                i++;
            };
        }

        private bool PathEquals(string path, string other) => PathComparer.Equals(path, other, PathCaseComparison.IgnoreCase, FolderPathEquality.RespectAmbiguity);

        private (string, string, string, T[]) Expect<T>(string path, string newPath = null, string oldPath = null, params T[] changeTypes) => (path, newPath, oldPath, changeTypes);

        private void PrepareRootFolder(MockWatchableFileSystem fileSystem, string path)
        {
            string rootFolder = path;
            while (true)
            {
                string folderPath = fileSystem.GetFolderPath(rootFolder);
                if (folderPath == null)
                    break;

                rootFolder = folderPath;
            }

            fileSystem.Create(fileSystem.UniqueFolder(rootFolder));
        }

        private void PrepareParentFolder(MockWatchableFileSystem fileSystem, string path)
        {
            fileSystem.Create(fileSystem.UniqueFolder(fileSystem.GetFolderPath(path)));
        }

        private void PreparePath(MockWatchableFileSystem fileSystem, string uniquePath)
        {
            PrepareParentFolder(fileSystem, uniquePath);
            fileSystem.Create(uniquePath);
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
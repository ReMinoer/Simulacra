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
        private const string ParentFolderPath = @"c:\folder\";
        private const string OldFileName = @"file.old";
        private const string NewFileName = @"file.new";
        private const string OldFolderName = @"oldfolder\";
        private const string NewFolderName = @"newfolder\";

        static private readonly object[] FileTestCases =
        {
            new TestCaseData(FilePath, FilePath),
            new TestCaseData(FilePath, @"c:\folder\*"),
            new TestCaseData(FilePath, @"c:\folder\*.ext"),
            new TestCaseData(FilePath, @"c:\folder\file.*"),
            new TestCaseData(FilePath, @"c:\folder\f*e.ext"),
            new TestCaseData(FilePath, @"c:\folder\f?le.?xt"),
        };

        // Mocking only work if watched paths only capture one part of the renaming
        static private readonly object[] RenameFileTestCases =
        {
            new TestCaseData(ParentFolderPath, OldFileName, NewFileName, OldFileName, NewFileName),
            new TestCaseData(ParentFolderPath, OldFileName, NewFileName, "*.old", "*.new"),
            new TestCaseData(ParentFolderPath, OldFileName, NewFileName, "f*e.old", "f*e.new"),
            new TestCaseData(ParentFolderPath, OldFileName, NewFileName, "f?le.?ld", "f?le.n?w"),
        };

        static private readonly object[] FolderTestCases =
        {
            new TestCaseData(FolderPath, FolderPath),
            new TestCaseData(FolderPath, @"c:\folder\subfolder"),
            new TestCaseData(FolderPath, @"c:\folder\*\"),
            new TestCaseData(FolderPath, @"c:\folder\s?bf?lder\"),
        };

        // Mocking only work if watched paths only capture one part of the renaming
        static private readonly object[] RenameFolderTestCases =
        {
            new TestCaseData(ParentFolderPath, OldFolderName, NewFolderName, OldFolderName, NewFolderName),
            new TestCaseData(ParentFolderPath, OldFolderName, NewFolderName, @"old*\", @"new*\"),
            new TestCaseData(ParentFolderPath, OldFolderName, NewFolderName, @"?ldf?lder\", @"n?wf?lder\"),
        };

        [TestCaseSource(nameof(FileTestCases))]
        public void ChangeFile(string path, string watchedPath)
        {
            TestFile(x => PreparePath(x, path), x => x.Change(path), Expect(watchedPath, path, null, null, FileChangeType.Edited));
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void CreateFile(string path, string watchedPath)
        {
            TestFile(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(watchedPath, path, null, null, FileChangeType.Created));
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void DeleteFile(string path, string watchedPath)
        {
            TestFile(x => PreparePath(x, path), x => x.Delete(path), Expect(watchedPath, path, null, null, FileChangeType.Deleted));
        }

        [TestCaseSource(nameof(RenameFileTestCases))]
        public void RenameFile(string folder, string oldName, string newName, string watchedOldName, string watchedNewName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;
            string watchedOldPath = folder + watchedOldName;
            string watchedNewPath = folder + watchedNewName;

            TestFile(x => PreparePath(x, oldPath),
                x => x.Rename(oldPath, newName),
                Expect(watchedOldPath, oldPath, newPath, null, FileChangeType.Deleted),
                Expect(watchedNewPath, newPath, null, oldPath, FileChangeType.Created));
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void QuicklyCreateDeleteFile(string path, string watchedPath)
        {
            TestFile(x => PrepareParentFolder(x, path), x =>
            {
                x.StartBatching();
                x.Create(path);
                x.Delete(path);
                x.StopBatching();
            }, Expect(watchedPath, path, null, null, FileChangeType.Created, FileChangeType.Deleted));
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void QuicklyDeleteCreateFile(string path, string watchedPath)
        {
            TestFile(x => PreparePath(x, path), x =>
            {
                x.StartBatching();
                x.Delete(path);
                x.Create(path);
                x.StopBatching();
            }, Expect(watchedPath, path, null, null, FileChangeType.Deleted, FileChangeType.Created));
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void CreateFolder(string path, string watchedPath)
        {
            TestFolder(x => PrepareParentFolder(x, path), x => x.Create(path), Expect(watchedPath, path, null, null, FolderChangeType.Created));
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void DeleteFolder(string path, string watchedPath)
        {
            TestFolder(x => PreparePath(x, path), x => x.Delete(path), Expect(watchedPath, path, null, null, FolderChangeType.Deleted));
        }

        [TestCaseSource(nameof(RenameFolderTestCases))]
        public void RenameFolder(string folder, string oldName, string newName, string watchedOldName, string watchedNewName)
        {
            string oldPath = folder + oldName;
            string newPath = folder + newName;
            string watchedOldPath = folder + watchedOldName;
            string watchedNewPath = folder + watchedNewName;

            TestFolder(x => PreparePath(x, oldPath),
                x => x.Rename(oldPath, newName),
                Expect(watchedOldPath, oldPath, newPath, null, FolderChangeType.Deleted),
                Expect(watchedNewPath, newPath, null, oldPath, FolderChangeType.Created));
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void QuicklyCreateDeleteFolder(string path, string watchedPath)
        {
            TestFolder(x => PrepareParentFolder(x, path), x =>
            {
                x.StartBatching();
                x.Create(path);
                x.Delete(path);
                x.StopBatching();
            }, Expect(watchedPath, path, null, null, FolderChangeType.Created, FolderChangeType.Deleted));
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void QuicklyDeleteCreateFolder(string path, string watchedPath)
        {
            TestFolder(x => PreparePath(x, path), x =>
            {
                x.StartBatching();
                x.Delete(path);
                x.Create(path);
                x.StopBatching();
            }, Expect(watchedPath, path, null, null, FolderChangeType.Deleted, FolderChangeType.Created));
        }

        [TestCase(@"c:\folder\", "file.ext", "file.ext")]
        public void WatchFileWithMissingFolder(string folder, string name, string watchedName)
        {
            string path = folder + name;
            string watchedPath = folder + watchedName;

            TestFile(x => PrepareRootFolder(x, path), x =>
            {
                x.Create(folder);
                x.Create(path);
                x.Change(path);
            }, Expect(watchedPath, path, null, null, FileChangeType.Created, FileChangeType.Edited));
        }

        [TestCase(@"c:\folder\", "file.ext", "file.ext")]
        public void RemoveFolderContainingWatchedFile(string folder, string name, string watchedName)
        {
            string path = folder + name;
            string watchedPath = folder + watchedName;

            TestFile(x => PreparePath(x, path), x => x.Delete(folder), Expect(watchedPath, path, null, null, FileChangeType.Deleted));
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void SuspendFileWatching(string path, string watchedPath)
        {
            TestBase(x => PrepareParentFolder(x, path), x =>
                {
                    x.Create(path);
                    x.Change(path);
                    x.Delete(path);
                },
                new []
                {
                    ExpectNothing<FileChangeType>(watchedPath)
                },
                GetFileHandler,
                (watcher, p, handler) =>
                {
                    watcher.WatchFile(p, handler);
                    watcher.SuspendWatching(p, handler);
                },
                (watcher, p, handler) =>
                {
                    watcher.Unwatch(p, handler);
                }
            );
        }

        [TestCaseSource(nameof(FileTestCases))]
        public void ResumeFileWatching(string path, string watchedPath)
        {
            TestBase(x => PrepareParentFolder(x, path), x =>
                {
                    x.Create(path);
                    x.Change(path);
                    x.Delete(path);
                },
                new[]
                {
                    Expect(watchedPath, path, null, null, FileChangeType.Created, FileChangeType.Edited, FileChangeType.Deleted)
                },
                GetFileHandler,
                (watcher, p, handler) =>
                {
                    watcher.WatchFile(p, handler);
                    watcher.SuspendWatching(p, handler).Dispose();
                },
                (watcher, p, handler) =>
                {
                    watcher.Unwatch(p, handler);
                }
            );
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void SuspendFolderWatching(string path, string watchedPath)
        {
            TestBase(x => PrepareParentFolder(x, path), x =>
                {
                    x.Create(path);
                    x.Delete(path);
                },
                new[]
                {
                    ExpectNothing<FolderChangeType>(watchedPath)
                },
                GetFolderHandler,
                (watcher, p, handler) =>
                {
                    watcher.WatchFolder(p, handler);
                    watcher.SuspendWatching(p, handler);
                },
                (watcher, p, handler) =>
                {
                    watcher.Unwatch(p, handler);
                }
            );
        }

        [TestCaseSource(nameof(FolderTestCases))]
        public void ResumeFolderWatching(string path, string watchedPath)
        {
            TestBase(x => PrepareParentFolder(x, path), x =>
                {
                    x.Create(path);
                    x.Delete(path);
                },
                new[]
                {
                    Expect(watchedPath, path, null, null, FolderChangeType.Created, FolderChangeType.Deleted)
                },
                GetFolderHandler,
                (watcher, p, handler) =>
                {
                    watcher.WatchFolder(p, handler);
                    watcher.SuspendWatching(p, handler).Dispose();
                },
                (watcher, p, handler) =>
                {
                    watcher.Unwatch(p, handler);
                }
            );
        }

        private void TestFile(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params ExceptedChange<FileChangeType>[] expectedChanges)
            => TestBase(
                prepareSystem, mockAction, expectedChanges, GetFileHandler,
                (watcher, path, handler) => watcher.WatchFile(path, handler),
                (watcher, path, handler) => watcher.Unwatch(path, handler)
            );

        private void TestFolder(Action<MockWatchableFileSystem> prepareSystem, Action<MockWatchableFileSystem> mockAction, params ExceptedChange<FolderChangeType>[] expectedChanges)
            => TestBase(
                prepareSystem, mockAction, expectedChanges, GetFolderHandler,
                (watcher, path, handler) => watcher.WatchFolder(path, handler),
                (watcher, path, handler) => watcher.Unwatch(path, handler)
            );

        private void TestBase<TChangeType, THandler>(
            Action<MockWatchableFileSystem> prepareSystem,
            Action<MockWatchableFileSystem> mockAction,
            ExceptedChange<TChangeType>[] expectedChanges,
            Func<PathWatcher, ExceptedChange<TChangeType>, Action, THandler> getHandlerFunc,
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
                (string, THandler)[] handlers = expectedChanges.Select((x, i) => (x.WatchedPath, getHandlerFunc(watcher, x, () => handled[i] = true))).ToArray();

                foreach ((string watchedPath, THandler handler) in handlers)
                    watchAction(watcher, watchedPath, handler);
                {
                    handled.Should().AllBeEquivalentTo(false);
                }

                mockAction(mockSystem);
                {
                    for (int i = 0; i < expectedChanges.Length; i++)
                        handled[i].Should().Be(!expectedChanges[i].NoChange);
                }

                for (int i = 0; i < handled.Length; i++)
                    handled[i] = false;

                foreach ((string watchedPath, THandler handler) in handlers)
                    unwatchAction(watcher, watchedPath, handler);
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

        private FileChangedEventHandler GetFileHandler(PathWatcher watcher, ExceptedChange<FileChangeType> exceptedChange, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                exceptedChange.NoChange.Should().BeFalse();

                sender.Should().Be(watcher);
                args.WatchedPathPattern.Pattern.Should().Match(x => PathEquals(x, exceptedChange.WatchedPath));
                args.Path.Should().Match(x => PathEquals(x, exceptedChange.Path));
                args.ChangeType.Should().Be(exceptedChange.ChangeTypes[i]);
                args.NewPath.Should().Match(x => PathEquals(x, exceptedChange.NewPath));
                args.OldPath.Should().Match(x => PathEquals(x, exceptedChange.OldPath));
                action();
                i++;
            };
        }

        private FolderChangedEventHandler GetFolderHandler(PathWatcher watcher, ExceptedChange<FolderChangeType> exceptedChange, Action action)
        {
            int i = 0;
            return (sender, args) =>
            {
                exceptedChange.NoChange.Should().BeFalse();
                
                sender.Should().Be(watcher);
                args.WatchedPathPattern.Pattern.Should().Match(x => PathEquals(x, exceptedChange.WatchedPath));
                args.Path.Should().Match(x => PathEquals(x, exceptedChange.Path));
                args.ChangeType.Should().Be(exceptedChange.ChangeTypes[i]);
                args.NewPath.Should().Match(x => PathEquals(x, exceptedChange.NewPath));
                args.OldPath.Should().Match(x => PathEquals(x, exceptedChange.OldPath));
                action();
                i++;
            };
        }

        private bool PathEquals(string path, string other) => PathComparer.Equals(path, other, PathCaseComparison.IgnoreCase, FolderPathEquality.RespectAmbiguity);

        private ExceptedChange<TChangeType> Expect<TChangeType>(string watchedPath, string path, string newPath = null, string oldPath = null, params TChangeType[] changeTypes)
        {
            return new ExceptedChange<TChangeType>
            {
                WatchedPath = watchedPath,
                Path = path,
                NewPath = newPath,
                OldPath = oldPath,
                ChangeTypes = changeTypes
            };
        }

        private ExceptedChange<TChangeType> ExpectNothing<TChangeType>(string watchedPath)
        {
            return new ExceptedChange<TChangeType>
            {
                WatchedPath = watchedPath
            };
        }

        private struct ExceptedChange<TChangeType>
        {
            public string WatchedPath { get; set; }
            public string Path { get; set; }
            public string NewPath { get; set; }
            public string OldPath { get; set; }
            public TChangeType[] ChangeTypes { get; set; }

            public bool NoChange => ChangeTypes == null;
        }

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
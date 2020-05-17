using System;
using System.IO;

namespace Simulacra.IO.Watching
{
    public interface IFileSystemWatcher : IShared, IDisposable
    {
        event FileSystemEventHandler Changed;
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Deleted;
        event RenamedEventHandler Renamed;
        void Enable();
    }
}
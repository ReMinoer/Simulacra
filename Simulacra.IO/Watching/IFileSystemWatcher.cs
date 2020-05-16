using System;
using System.IO;

namespace Simulacra.IO.Watching
{
    public interface IFileSystemWatcher : IDisposable
    {
        bool EnableRaisingEvents { set; }
        event FileSystemEventHandler Changed;
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Deleted;
        event RenamedEventHandler Renamed;
    }
}
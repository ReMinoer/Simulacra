using System;

namespace Simulacra.IO.Watching
{
    public interface IFileSystemWatcher : IShared, IDisposable
    {
        event FileSystemChangedEventHandler Changed;
        event FileSystemChangedEventHandler Created;
        event FileSystemChangedEventHandler Deleted;
        event FileSystemRenamedEventHandler Renamed;
        void Enable();
    }
}
using System;

namespace Simulacra.IO.Watching
{
    public interface IShared
    {
        event EventHandler FullyReleased;
        void Increment();
        void Release();
    }
}
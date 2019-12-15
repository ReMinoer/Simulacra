using System;

namespace Simulacra.Utils
{
    public interface INotifyArrayChanged
    {
        event EventHandler<ArrayChangedEventArgs> ArrayChanged;
    }
}
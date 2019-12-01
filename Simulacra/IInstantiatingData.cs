using System;

namespace Simulacra
{
    public interface IInstantiatingData : IBindableData, IDisposable
    {
        bool IsInstantiated { get; }
        void Instantiate();
    }

    public interface IInstantiatingData<out T> : IInstantiatingData, IBindableData<T>
    {
    }
}
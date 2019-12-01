namespace Simulacra
{
    public interface IBindableData
    {
        object BindedObject { get; }
    }
    
    public interface IBindableData<out T> : IBindableData
    {
        new T BindedObject { get; }
    }
}
namespace Simulacra
{
    public interface IDataBindable
    {
        object BindedObject { get; }
    }
    
    public interface IDataBindable<out T> : IDataBindable
    {
        new T BindedObject { get; }
    }
}
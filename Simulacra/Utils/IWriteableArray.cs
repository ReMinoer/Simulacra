namespace Simulacra.Utils
{
    public interface IWriteableArray<T> : IArray<T>
    {
        new T this[params int[] indexes] { get; set; }
    }
}
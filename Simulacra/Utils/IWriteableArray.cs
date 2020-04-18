namespace Simulacra.Utils
{
    public interface IWriteableArray<T> : IArray<T>
    {
        T this[params int[] indexes] { get; set; }
    }

    public interface IOneDimensionWriteableArray<T> : IWriteableArray<T>, IOneDimensionArray<T>
    {
        T this[int i] { get; set; }
    }

    public interface ITwoDimensionWriteableArray<T> : IWriteableArray<T>, ITwoDimensionArray<T>
    {
        T this[int i, int j] { get; set; }
    }

    public interface IThreeDimensionWriteableArray<T> : IWriteableArray<T>, IThreeDimensionArray<T>
    {
        T this[int i, int j, int k] { get; set; }
    }

    public interface IFourDimensionWriteableArray<T> : IWriteableArray<T>, IFourDimensionArray<T>
    {
        T this[int i, int j, int k, int l] { get; set; }
    }
}
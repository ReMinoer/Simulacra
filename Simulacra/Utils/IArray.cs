using System.Collections.Generic;

namespace Simulacra.Utils
{
    public interface IArrayDefinition
    {
        int Rank { get; }
        int GetLength(int dimension);
    }

    public interface IArray : IArrayDefinition
    {
        object this[params int[] indexes] { get; }
    }

    public interface IOneDimensionArray : IArray
    {
        object this[int i] { get; }
    }

    public interface ITwoDimensionArray : IArray
    {
        object this[int i, int j] { get; }
    }

    public interface IThreeDimensionArray : IArray
    {
        object this[int i, int j, int k] { get; }
    }

    public interface IFourDimensionArray : IArray
    {
        object this[int i, int j, int k, int l] { get; }
    }

    public interface IArray<out T> : IArray, IEnumerable<T>
    {
        new T this[params int[] indexes] { get; }
    }

    public interface IOneDimensionArray<out T> : IOneDimensionArray, IArray<T>
    {
        new T this[int i] { get; }
    }

    public interface ITwoDimensionArray<out T> : ITwoDimensionArray, IArray<T>
    {
        new T this[int i, int j] { get; }
    }

    public interface IThreeDimensionArray<out T> : IThreeDimensionArray, IArray<T>
    {
        new T this[int i, int j, int k] { get; }
    }

    public interface IFourDimensionArray<out T> : IFourDimensionArray, IArray<T>
    {
        new T this[int i, int j, int k, int l] { get; }
    }
}
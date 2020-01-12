using System.Collections.Generic;

namespace Simulacra.Utils
{
    public interface IArray
    {
        int Rank { get; }
        int GetLength(int dimension);
    }

    public interface IArray<out T> : IArray, IEnumerable<T>
    {
        T this[params int[] indexes] { get; }
    }

    public interface IOneDimensionArray<out T> : IArray<T>
    {
        T this[int i] { get; }
    }

    public interface ITwoDimensionArray<out T> : IArray<T>
    {
        T this[int i, int j] { get; }
    }

    public interface IThreeDimensionArray<out T> : IArray<T>
    {
        T this[int i, int j, int k] { get; }
    }

    public interface IFourDimensionArray<out T> : IArray<T>
    {
        T this[int i, int j, int k, int l] { get; }
    }
}
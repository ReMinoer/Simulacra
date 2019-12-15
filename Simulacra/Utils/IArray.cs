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
}
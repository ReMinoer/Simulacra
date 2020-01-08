using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    static public class ArrayExtension
    {
        static public IWriteableArray<T> AsGeneric<T>(this T[] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[][] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[][][] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[][][][] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[,] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[,,] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this T[,,,] array) => new Array<T>(array);
        static public IWriteableArray<T> AsGeneric<T>(this Array array) => new Array<T>(array);

        static public IEnumerable<int[]> Indexes(this Array array)
        {
            var indexes = new int[array.Rank];
            do
            {
                yield return indexes;
            }
            while (array.MoveToNextIndex(indexes));
        }

        static public IEnumerable<int[]> Indexes(this IArray array)
        {
            var indexes = new int[array.Rank];
            do
            {
                yield return indexes;
            }
            while (array.MoveToNextIndex(indexes));
        }

        static public IEnumerable<int> Lengths(this Array array)
        {
            return Enumerable.Range(0, array.Rank).Select(array.GetLength);
        }

        static public IEnumerable<int> Lengths(this IArray array)
        {
            return Enumerable.Range(0, array.Rank).Select(array.GetLength);
        }

        static public bool MoveToNextIndex(this Array array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetLength(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = 0;
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool MoveToNextIndex(this IArray array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetLength(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = 0;
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool ContainsIndexes(this IArray array, params int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                if (indexes[r] < 0 || indexes[r] >= array.GetLength(r))
                    return false;

            return true;
        }
    }
}
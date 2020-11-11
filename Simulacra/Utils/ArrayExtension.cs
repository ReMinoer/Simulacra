using System;
using System.Linq;

namespace Simulacra.Utils
{
    static public class ArrayExtension
    {
        static public OneDimensionArray<T> AsGeneric<T>(this T[] array) => new OneDimensionArray<T>(array);
        static public TwoDimensionArray<T> AsGeneric<T>(this T[,] array) => new TwoDimensionArray<T>(array);
        static public ThreeDimensionArray<T> AsGeneric<T>(this T[,,] array) => new ThreeDimensionArray<T>(array);
        static public FourDimensionArray<T> AsGeneric<T>(this T[,,,] array) => new FourDimensionArray<T>(array);
        static public UnknownDimensionArray<T> AsGeneric<T>(this Array array) => new UnknownDimensionArray<T>(array);

        static public int[] Lengths(this Array array)
        {
            var lengths = new int[array.Rank];

            for (int i = 0; i < array.Rank; i++)
                lengths[i] = array.GetLength(i);

            return lengths;
        }

        static public int[] Lengths(this IArrayDefinition array)
        {
            var lengths = new int[array.Rank];

            for (int i = 0; i < array.Rank; i++)
                lengths[i] = array.GetLength(i);

            return lengths;
        }

        static public IndexRange IndexRange(this Array array)
        {
            return new IndexRange(new int[array.Rank], array.Lengths());
        }

        static public IndexRange IndexRange(this IArrayDefinition array)
        {
            return new IndexRange(new int[array.Rank], array.Lengths());
        }

        static public int[] GetInitialIndex(this IIndexEnumerator indexEnumerator)
        {
            int[] indexes = indexEnumerator.GetResetIndex();
            indexEnumerator.MoveIndex(indexes);
            return indexes;
        }

        static public int[] GetInitialIndex(this IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            int[] indexes = indexEnumerator.GetResetIndex();
            while (indexEnumerator.MoveIndex(indexes))
            {
                if (!excludingMasks.ContainsIndexes(indexes))
                    break;
            }

            return indexes;
        }

        static public bool MoveIndex(this IIndexEnumerator indexEnumerator, int[] indexes, IArrayMask[] excludingMasks = null)
        {
            do
            {
                if (!indexEnumerator.MoveIndex(indexes))
                    return false;
            }
            while (excludingMasks != null && excludingMasks.ContainsIndexes(indexes));

            return true;
        }

        static public bool ContainsIndexes(this Array array, params int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                if (indexes[r] < 0 || indexes[r] >= array.GetLength(r))
                    return false;

            return true;
        }

        static public bool ContainsIndexes(this IArrayDefinition array, params int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                if (indexes[r] < 0 || indexes[r] >= array.GetLength(r))
                    return false;

            return true;
        }

        static private bool ContainsIndexes(this IArrayMask[] masks, int[] indexes)
        {
            for (int i = 0; i < masks.Length; i++)
                if (masks[i].ContainsIndex(indexes))
                    return true;

            return false;
        }

        static public IArray<TNewValue> Retype<TOldValue, TNewValue>(this IArray<TOldValue> array, Func<TOldValue, TNewValue> getter)
        {
            return new RetypedArray<TOldValue, TNewValue>(array, getter);
        }

        static public IWriteableArray<TNewValue> Retype<TOldValue, TNewValue>(this IWriteableArray<TOldValue> array, Func<TOldValue, TNewValue> getter, Action<TOldValue, TNewValue> setter)
        {
            return new RetypedWriteableArray<TOldValue, TNewValue>(array, getter, setter);
        }

        static public void Fill<T>(this Array array, T value, IIndexEnumerator indexEnumerator = null, IArrayMask[] excludingMasks = null)
        {
            IIndexEnumerator enumerator = indexEnumerator ?? array.IndexRange();

            int[] indexes = enumerator.GetResetIndex();
            while (enumerator.MoveIndex(indexes, excludingMasks))
                array.SetValue(value, indexes);
        }

        static public void Fill<T>(this Array array, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator = null, IArrayMask[] excludingMasks = null)
        {
            IIndexEnumerator enumerator = indexEnumerator ?? array.IndexRange();

            int[] indexes = enumerator.GetResetIndex();
            while (enumerator.MoveIndex(indexes, excludingMasks))
                array.SetValue(valueFactory((T)array.GetValue(indexes), indexes), indexes);
        }

        static public void Fill<T>(this IWriteableArray<T> array, T value, IIndexEnumerator indexEnumerator = null, IArrayMask[] excludingMasks = null)
        {
            IIndexEnumerator enumerator = indexEnumerator ?? array.IndexRange();

            int[] indexes = enumerator.GetResetIndex();
            while (enumerator.MoveIndex(indexes, excludingMasks))
                array[indexes] = value;
        }

        static public void Fill<T>(this IWriteableArray<T> array, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator = null, IArrayMask[] excludingMasks = null)
        {
            IIndexEnumerator enumerator = indexEnumerator ?? array.IndexRange();

            int[] indexes = enumerator.GetResetIndex();
            while (enumerator.MoveIndex(indexes, excludingMasks))
                array[indexes] = valueFactory(array[indexes], indexes);
        }

        static public Array ToResizedArray<T>(this Array array, int[] newLengths, bool keepValues = true, Func<T, int[], T> defaultValueFactory = null, int[] rangeToFill = null)
        {
            var newArray = Array.CreateInstance(typeof(T), newLengths);
            int[] fillLengths = rangeToFill ?? newArray.Lengths();

            IArrayMask[] excludingMasks = null;
            if (keepValues)
            {
                int[] copyLengths = array.Lengths().Zip(fillLengths, Math.Min).ToArray();

                int copyRowsCount = copyLengths[0];
                int copyRowLength = copyLengths.Skip(1).Sum();
                int arrayRowLength = array.Lengths().Skip(1).Sum();
                int newArrayRowLength = newArray.Lengths().Skip(1).Sum();

                for (int i = 0; i < copyRowsCount; ++i)
                    Array.Copy(array, i * arrayRowLength, newArray, i * newArrayRowLength, copyRowLength);

                excludingMasks = new IArrayMask[] {new IndexRange(copyLengths)};
            }

            newArray.Fill(defaultValueFactory ?? ((_, __) => default), new IndexRange(fillLengths), excludingMasks);

            return newArray;
        }
    }
}
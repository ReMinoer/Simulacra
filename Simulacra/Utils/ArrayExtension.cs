﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    static public class ArrayExtension
    {
        static public ArrayDefinition GetArrayDefinition(this Array array) => new ArrayDefinition(array);

        static public OneDimensionArray<T> AsGeneric<T>(this T[] array) => new OneDimensionArray<T>(array);
        static public TwoDimensionArray<T> AsGeneric<T>(this T[,] array) => new TwoDimensionArray<T>(array);
        static public ThreeDimensionArray<T> AsGeneric<T>(this T[,,] array) => new ThreeDimensionArray<T>(array);
        static public FourDimensionArray<T> AsGeneric<T>(this T[,,,] array) => new FourDimensionArray<T>(array);
        static public UnknownDimensionArray<T> AsGeneric<T>(this Array array) => new UnknownDimensionArray<T>(array);

        static public int GetUpperBound(this IArrayDefinition array, int dimension)
        {
            return array.GetLowerBound(dimension) + array.GetLength(dimension) - 1;
        }

        static public int GetOutOfBound(this Array array, int dimension)
        {
            return array.GetLowerBound(dimension) + array.GetLength(dimension);
        }

        static public int GetOutOfBound(this IArrayDefinition array, int dimension)
        {
            return array.GetLowerBound(dimension) + array.GetLength(dimension);
        }

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

        static public int[] GetStartingIndex(this Array array)
        {
            var indexes = new int[array.Rank];
            array.GetStartingIndex(indexes);
            return indexes;
        }

        static public int[] GetStartingIndex(this IArrayDefinition array)
        {
            var indexes = new int[array.Rank];
            array.GetStartingIndex(indexes);
            return indexes;
        }

        static public void GetStartingIndex(this Array array, int[] indexes)
        {
            for (int i = 0; i < array.Rank; i++)
                indexes[i] = array.GetLowerBound(i);
        }

        static public void GetStartingIndex(this IArrayDefinition array, int[] indexes)
        {
            for (int i = 0; i < array.Rank; i++)
                indexes[i] = array.GetLowerBound(i);
        }

        static public int[] GetEndingIndex(this Array array)
        {
            var indexes = new int[array.Rank];
            array.GetEndingIndex(indexes);
            return indexes;
        }

        static public int[] GetEndingIndex(this IArrayDefinition array)
        {
            var indexes = new int[array.Rank];
            array.GetEndingIndex(indexes);
            return indexes;
        }

        static public void GetEndingIndex(this Array array, int[] indexes)
        {
            for (int i = 0; i < array.Rank; i++)
                indexes[i] = array.GetUpperBound(i);
        }

        static public void GetEndingIndex(this IArrayDefinition array, int[] indexes)
        {
            for (int i = 0; i < array.Rank; i++)
                indexes[i] = array.GetUpperBound(i);
        }

        static public int[] GetResetIndex(this Array array)
        {
            var indexes = array.GetStartingIndex();
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public int[] GetResetIndex(this IArrayDefinition array)
        {
            var indexes = array.GetStartingIndex();
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public int[] GetResetIndex(this Array array, int[] indexes)
        {
            array.GetStartingIndex(indexes);
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public int[] GetResetIndex(this IArrayDefinition array, int[] indexes)
        {
            array.GetStartingIndex(indexes);
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public bool MoveIndex(this Array array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetOutOfBound(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = 0;
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool MoveIndex(this IArrayDefinition array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetOutOfBound(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = 0;
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool MoveIndex(this Array array, int[] indexes, int dimension)
        {
            indexes[dimension]++;
            if (indexes[dimension] < array.GetOutOfBound(dimension))
                return true;

            indexes[dimension] = array.GetLowerBound(dimension);
            return false;
        }

        static public bool MoveIndex(this IArrayDefinition array, int[] indexes, int dimension)
        {
            indexes[dimension]++;
            if (indexes[dimension] < array.GetOutOfBound(dimension))
                return true;

            indexes[dimension] = array.GetLowerBound(dimension);
            return false;
        }

        static public bool ContainsIndex(this Array array, params int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                if (indexes[r] < array.GetLowerBound(r) || indexes[r] > array.GetUpperBound(r))
                    return false;

            return true;
        }

        static public bool ContainsIndex(this IArrayDefinition array, params int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                if (indexes[r] < array.GetLowerBound(r) || indexes[r] > array.GetUpperBound(r))
                    return false;

            return true;
        }

        static public IEnumerable<int[]> Indexes(this IArrayDefinition array)
        {
            return new IndexEnumerable(array);
        }

        static public IArray<TNewValue> Retype<TOldValue, TNewValue>(this IArray<TOldValue> array, Func<TOldValue, TNewValue> getter)
        {
            return new RetypedArray<TOldValue, TNewValue>(array, getter);
        }

        static public IWriteableArray<TNewValue> Retype<TOldValue, TNewValue>(this IWriteableArray<TOldValue> array, Func<TOldValue, TNewValue> getter, Action<TOldValue, TNewValue> setter)
        {
            return new RetypedWriteableArray<TOldValue, TNewValue>(array, getter, setter);
        }

        static public void Fill<T>(this Array array, T value, IEnumerable<int[]> indexes = null)
        {
            IEnumerable<int[]> indexEnumerable = indexes ?? array.GetArrayDefinition().Indexes();

            foreach (int[] index in indexEnumerable)
                array.SetValue(value, index);
        }

        static public void Fill<T>(this Array array, Func<T, int[], T> valueFactory, IEnumerable<int[]> indexes = null)
        {
            IEnumerable<int[]> indexEnumerable = indexes ?? array.GetArrayDefinition().Indexes();

            foreach (int[] index in indexEnumerable)
                array.SetValue(valueFactory((T)array.GetValue(index), index), index);
        }

        static public void Fill<T>(this IWriteableArray<T> array, T value, IEnumerable<int[]> indexes = null)
        {
            IEnumerable<int[]> indexEnumerable = indexes ?? array.Indexes();

            foreach (int[] index in indexEnumerable)
                array[index] = value;
        }

        static public void Fill<T>(this IWriteableArray<T> array, Func<T, int[], T> valueFactory, IEnumerable<int[]> indexes = null)
        {
            IEnumerable<int[]> indexEnumerable = indexes ?? array.Indexes();

            foreach (int[] index in indexEnumerable)
                array[index] = valueFactory(array[index], index);
        }

        static public bool IndexesIntersects(this IArrayDefinition first, IArrayDefinition second, out IArrayDefinition intersection)
        {
            int[] minimums = first.GetStartingIndex().Zip(second.GetStartingIndex(), Math.Max).ToArray();
            int[] maximums = first.GetEndingIndex().Zip(second.GetEndingIndex(), Math.Min).ToArray();

            if (Enumerable.Range(0, first.Rank).Any(x => minimums[x] > maximums[x]))
            {
                intersection = new IndexRange();
                return false;
            }

            intersection = new IndexRange(minimums, maximums.Zip(minimums, (x, y) => x - y + 1).ToArray());
            return true;
        }

        static public Array ToResizedArray<T>(this Array array, int[] newLengths, bool keepValues = true, Func<T, int[], T> defaultValueFactory = null, int[] rangeToFill = null)
        {
            var newArray = Array.CreateInstance(typeof(T), newLengths);
            int[] fillLengths = rangeToFill ?? newArray.Lengths();

            IArrayDefinition excludingMask = null;
            if (keepValues)
            {
                int[] copyLengths = array.Lengths().Zip(fillLengths, Math.Min).ToArray();

                int copyRowsCount = copyLengths[0];
                int copyRowLength = copyLengths.Skip(1).Sum();
                int arrayRowLength = array.Lengths().Skip(1).Sum();
                int newArrayRowLength = newArray.Lengths().Skip(1).Sum();

                for (int i = 0; i < copyRowsCount; ++i)
                    Array.Copy(array, i * arrayRowLength, newArray, i * newArrayRowLength, copyRowLength);

                excludingMask = new IndexRange(copyLengths);
            }

            defaultValueFactory = defaultValueFactory ?? ((_, __) => default);

            IEnumerable<int[]> fillIndexes = new IndexRange(fillLengths).Indexes();
            if (excludingMask != null)
                fillIndexes = fillIndexes.Where(x => !excludingMask.ContainsIndex(x));

            newArray.Fill(defaultValueFactory, fillIndexes);

            return newArray;
        }
    }
}
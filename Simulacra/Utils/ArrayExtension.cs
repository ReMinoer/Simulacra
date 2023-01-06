using System;
using System.Collections;
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

            for (int r = 0; r < array.Rank; r++)
                lengths[r] = array.GetLength(r);

            return lengths;
        }

        static public int[] Lengths(this IArrayDefinition array)
        {
            var lengths = new int[array.Rank];

            for (int r = 0; r < array.Rank; r++)
                lengths[r] = array.GetLength(r);

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
            for (int r = 0; r < array.Rank; r++)
                indexes[r] = array.GetLowerBound(r);
        }

        static public void GetStartingIndex(this IArrayDefinition array, int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                indexes[r] = array.GetLowerBound(r);
        }

        static public void GetStartingIndex(this Array array, out int i)
        {
            i = array.GetLowerBound(0);
        }

        static public void GetStartingIndex(this Array array, out int i, out int j)
        {
            i = array.GetLowerBound(0);
            j = array.GetLowerBound(1);
        }

        static public void GetStartingIndex(this IArrayDefinition array, out int i)
        {
            i = array.GetLowerBound(0);
        }

        static public void GetStartingIndex(this IArrayDefinition array, out int i, out int j)
        {
            i = array.GetLowerBound(0);
            j = array.GetLowerBound(1);
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
            for (int r = 0; r < array.Rank; r++)
                indexes[r] = array.GetUpperBound(r);
        }

        static public void GetEndingIndex(this IArrayDefinition array, int[] indexes)
        {
            for (int r = 0; r < array.Rank; r++)
                indexes[r] = array.GetUpperBound(r);
        }

        static public int[] GetResetIndex(this Array array)
        {
            int[] indexes = array.GetStartingIndex();
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public int[] GetResetIndex(this IArrayDefinition array)
        {
            int[] indexes = array.GetStartingIndex();
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public void GetResetIndex(this Array array, int[] indexes)
        {
            array.GetStartingIndex(indexes);
            indexes[array.Rank - 1]--;
        }

        static public void GetResetIndex(this IArrayDefinition array, int[] indexes)
        {
            array.GetStartingIndex(indexes);
            indexes[array.Rank - 1]--;
        }

        static public void GetResetIndex(this Array array, out int i)
        {
            GetStartingIndex(array, out i);
            i--;
        }

        static public void GetResetIndex(this Array array, out int i, out int j)
        {
            GetStartingIndex(array, out i, out j);
            j--;
        }

        static public void GetResetIndex(this IArrayDefinition array, out int i)
        {
            GetStartingIndex(array, out i);
            i--;
        }

        static public void GetResetIndex(this IArrayDefinition array, out int i, out int j)
        {
            GetStartingIndex(array, out i, out j);
            j--;
        }

        static public bool MoveIndex(this Array array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            // Check indexes of previous dimension are in bounds.
            for (int r = 0; r < array.Rank - 1; r++)
                if (indexes[r] >= array.GetOutOfBound(r))
                    return false;
            
            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetOutOfBound(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = array.GetLowerBound(r);
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool MoveIndex(this IArrayDefinition array, int[] indexes)
        {
            indexes[array.Rank - 1]++;
            
            // Check indexes of previous dimension are in bounds.
            for (int r = 0; r < array.Rank - 1; r++)
                if (indexes[r] >= array.GetOutOfBound(r))
                    return false;
            
            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetOutOfBound(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = array.GetLowerBound(r);
                indexes[r - 1]++;
            }

            return true;
        }

        static public bool MoveIndex(this Array array, ref int i)
        {
            i++;
            return i < array.GetOutOfBound(0);
        }

        static public bool MoveIndex(this Array array, ref int i, ref int j)
        {
            j++;

            // Check indexes of previous dimension are in bounds.
            if (i >= array.GetOutOfBound(0))
                return false;
            
            if (j < array.GetOutOfBound(1))
                return true;
            j = array.GetLowerBound(1);

            i++;
            return i < array.GetOutOfBound(0);
        }

        static public bool MoveIndex(this IArrayDefinition array, ref int i)
        {
            i++;
            return i < array.GetOutOfBound(0);
        }

        static public bool MoveIndex(this IArrayDefinition array, ref int i, ref int j)
        {
            j++;

            // Check indexes of previous dimension are in bounds.
            if (i >= array.GetOutOfBound(0))
                return false;

            if (j < array.GetOutOfBound(1))
                return true;
            j = array.GetLowerBound(1);

            i++;
            return i < array.GetOutOfBound(0);
        }

        static public bool MoveIndex(this Array array, ref int x, int dimension)
        {
            x++;
            if (x < array.GetOutOfBound(dimension))
                return true;

            x = array.GetLowerBound(dimension);
            return false;
        }

        static public bool MoveIndex(this IArrayDefinition array, ref int x, int dimension)
        {
            x++;
            if (x < array.GetOutOfBound(dimension))
                return true;

            x = array.GetLowerBound(dimension);
            return false;
        }

        static public bool MoveIndex(this Array array, int[] indexes, int dimension)
        {
            return MoveIndex(array, ref indexes[dimension], dimension);
        }

        static public bool MoveIndex(this IArrayDefinition array, int[] indexes, int dimension)
        {
            return MoveIndex(array, ref indexes[dimension], dimension);
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

        static public TwoDimensionIndexSequenceCollection ToSequences<T>(this IEnumerable<int[]> indexes, IArrayDefinition array)
        {
            var sequences = new List<TwoDimensionIndexSequence>();
            int sequenceI = 0;
            int sequenceJ = 0;
            int length = 0;

            int[] predictedIndex = null;
            foreach (int[] index in indexes)
            {
                if (predictedIndex != null && !predictedIndex.Equals(index))
                {
                    sequences.Add(new TwoDimensionIndexSequence(sequenceI, sequenceJ, length));
                    length = 0;
                }

                if (length == 0)
                {
                    sequenceI = index[0];
                    sequenceJ = index[1];
                }

                length++;

                if (predictedIndex == null)
                    predictedIndex = new int[2];

                Array.Copy(index, predictedIndex, index.Length);
                array.MoveIndex(predictedIndex, 0);
            }

            return new TwoDimensionIndexSequenceCollection(sequences);
        }
    }

    public class TwoDimensionIndexSequenceCollection : IEnumerable<int[]>
    {
        private readonly IList<TwoDimensionIndexSequence> _sequences;
        public bool IsEmpty => _sequences.Count == 0;

        public TwoDimensionIndexSequenceCollection(IList<TwoDimensionIndexSequence> orderedSequences)
        {
            _sequences = orderedSequences;
        }

        public IEnumerator<int[]> GetEnumerator() => new Enumerator(_sequences);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<int[]>
        {
            private readonly IList<TwoDimensionIndexSequence> _sequencesArray;

            public int[] Current { get; }
            object IEnumerator.Current => Current;

            private int _arrayIndex;
            private int _sequenceIndex;

            public Enumerator(IList<TwoDimensionIndexSequence> sequencesArray)
            {
                _sequencesArray = sequencesArray;
                Current = new int[2];
                Reset();
            }

            public void Reset()
            {
                _arrayIndex = 0;
                _sequenceIndex = -1;

                if (_sequencesArray.Count == 0)
                {
                    Current[0] = 0;
                    Current[1] = -1;
                    return;
                }

                Current[0] = _sequencesArray[0].I;
                Current[1] = _sequencesArray[0].J - 1;
            }

            public bool MoveNext()
            {
                if (_sequencesArray.Count == 0)
                    return false;

                _sequenceIndex++;
                if (_sequenceIndex >= _sequencesArray[_arrayIndex].Length)
                {
                    _sequenceIndex = 0;
                    _arrayIndex++;
                    if (_arrayIndex >= _sequencesArray.Count)
                        return false;
                }

                TwoDimensionIndexSequence sequence = _sequencesArray[_arrayIndex];
                Current[0] = sequence.I;
                Current[1] = sequence.J + _sequenceIndex;
                return true;
            }

            public void Dispose() { }
        }
    }

    public struct TwoDimensionIndexSequence : IComparable<TwoDimensionIndexSequence>
    {
        public int I { get; }
        public int J { get; }
        public int Length { get; }

        public TwoDimensionIndexSequence(int i, int j, int length)
        {
            I = i;
            J = j;
            Length = length;
        }

        public int CompareTo(TwoDimensionIndexSequence other)
        {
            int result = I.CompareTo(other.I);
            if (result != 0)
                return result;

            return J.CompareTo(other.J);
        }
    }
}
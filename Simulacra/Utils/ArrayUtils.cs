using System;
using System.Collections;
using System.Collections.Generic;

namespace Simulacra.Utils
{
    public class ArrayUtils
    {
        static public IEnumerable<int[]> CreateIndexEnumerable(Array array) => new IndexEnumerable(array.Rank, array.GetLength);
        static public IEnumerator<int[]> CreateIndexEnumerator(Array array) => new IndexEnumerator(array.Rank, array.GetLength);
        static public IEnumerable<int[]> CreateIndexEnumerable(IArrayDefinition array) => new IndexEnumerable(array.Rank, array.GetLength);
        static public IEnumerator<int[]> CreateIndexEnumerator(IArrayDefinition array) => new IndexEnumerator(array.Rank, array.GetLength);
        
        private class IndexEnumerable : IEnumerable<int[]>
        {
            private readonly int _rank;
            private readonly Func<int, int> _lengthFunc;

            public IndexEnumerable(int rank, Func<int, int> lengthFunc)
            {
                _rank = rank;
                _lengthFunc = lengthFunc;
            }

            public IEnumerator<int[]> GetEnumerator() => new IndexEnumerator(_rank, _lengthFunc);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class IndexEnumerator : IEnumerator<int[]>
        {
            private readonly int _rank;
            private readonly Func<int, int> _lengthFunc;

            public int[] Current { get; }
            object IEnumerator.Current => Current;

            public IndexEnumerator(int rank, Func<int, int> lengthFunc)
            {
                _rank = rank;
                _lengthFunc = lengthFunc;

                Current = new int[_rank];
                Reset();
            }

            public void Reset()
            {
                Current.Initialize();
                Current[_rank - 1]--;
            }

            public bool MoveNext()
            {
                Current[_rank - 1]++;

                for (int r = _rank - 1; r >= 0; r--)
                {
                    if (Current[r] < _lengthFunc(r))
                        break;

                    if (r - 1 < 0)
                        return false;

                    Current[r] = 0;
                    Current[r - 1]++;
                }

                return true;
            }

            public void Dispose() { }
        }
    }
}
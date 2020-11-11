using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    public readonly struct IndexRange : IIndexEnumerator, IArrayMask
    {
        public int[] StartingIndexes { get; }
        public int[] Lengths { get; }

        public int Rank => Lengths.Length;
        private IEnumerable<int> EndingIndexes => StartingIndexes.Zip(Lengths, (x, y) => x + y - 1);

        public IndexRange(int[] lengths)
            : this(new int[lengths.Length], lengths)
        {
        }

        public IndexRange(int[] startingIndexes, int[] lengths)
        {
            StartingIndexes = startingIndexes;
            Lengths = lengths;
        }

        public int[] GetResetIndex()
        {
            var indexes = new int[Rank];
            Array.Copy(StartingIndexes, indexes, StartingIndexes.Length);

            indexes[Rank - 1]--;
            return indexes;
        }

        public bool MoveIndex(int[] indexes)
        {
            indexes[Rank - 1]++;

            for (int r = Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < StartingIndexes[r] + Lengths[r])
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = StartingIndexes[r];
                indexes[r - 1]++;
            }

            return true;
        }

        public bool ContainsIndex(int[] indexes)
        {
            for (int i = 0; i < Rank; i++)
                if (indexes[i] < StartingIndexes[i] || indexes[i] >= StartingIndexes[i] + Lengths[i])
                    return false;

            return true;
        }

        public bool Intersects(IndexRange other, out IndexRange intersection)
        {
            int[] minimums = StartingIndexes.Zip(other.StartingIndexes, Math.Max).ToArray();
            int[] maximums = EndingIndexes.Zip(other.EndingIndexes, Math.Min).ToArray();

            if (Enumerable.Range(0, Rank).Any(x => minimums[x] > maximums[x]))
            {
                intersection = new IndexRange();
                return false;
            }

            intersection = new IndexRange(minimums, maximums.Zip(minimums, (x, y) => x - y + 1).ToArray());
            return true;
        }
    }
}
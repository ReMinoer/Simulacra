using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    public readonly struct ArrayRange : IArrayMask
    {
        public int[] StartingIndexes { get; }
        public int[] Lengths { get; }

        public int Rank => Lengths.Length;
        private IEnumerable<int> EndingIndexes => StartingIndexes.Zip(Lengths, (x, y) => x + y - 1);

        public ArrayRange(int[] lengths)
            : this(new int[lengths.Length], lengths)
        {
        }

        public ArrayRange(int[] startingIndexes, int[] lengths)
        {
            StartingIndexes = startingIndexes;
            Lengths = lengths;
        }

        public bool ContainsIndex(int[] indexes)
        {
            for (int i = 0; i < Rank; i++)
                if (indexes[i] < StartingIndexes[i] || indexes[i] >= StartingIndexes[i] + Lengths[i])
                    return false;

            return true;
        }

        public bool Intersects(ArrayRange other, out ArrayRange intersection)
        {
            int[] minimums = StartingIndexes.Zip(other.StartingIndexes, Math.Max).ToArray();
            int[] maximums = EndingIndexes.Zip(other.EndingIndexes, Math.Min).ToArray();

            if (Enumerable.Range(0, Rank).Any(x => minimums[x] > maximums[x]))
            {
                intersection = new ArrayRange();
                return false;
            }

            intersection = new ArrayRange(minimums, maximums.Zip(minimums, (x, y) => x - y + 1).ToArray());
            return true;
        }
    }
}
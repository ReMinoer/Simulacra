namespace Simulacra.Utils
{
    public readonly struct IndexRange : IArrayDefinition
    {
        public int[] StartingIndex { get; }
        public int[] Lengths { get; }
        public int Rank => Lengths.Length;

        public IndexRange(int[] lengths)
            : this(new int[lengths.Length], lengths)
        {
        }

        public IndexRange(int[] startingIndexes, int[] lengths)
        {
            StartingIndex = startingIndexes;
            Lengths = lengths;
        }

        int IArrayDefinition.GetLowerBound(int dimension) => StartingIndex[dimension];
        int IArrayDefinition.GetLength(int dimension) => Lengths[dimension];
    }
}
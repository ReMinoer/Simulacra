using System;

namespace Simulacra.Utils
{
    public enum ArrayChangedAction
    {
        Replace,
        Resize,
        Add,
        Remove,
        Move
    }

    public class ArrayChangedEventArgs : EventArgs
    {
        public ArrayChangedAction Action { get; private set; }

        public int[] StartingIndexes { get; private set; }
        public Array NewValues { get; private set; }

        public int[] OldStartingIndexes { get; private set; }
        public Array OldValues { get; private set; }

        public int SliceDimension { get; private set; }
        public int SliceCount { get; private set; }

        public int[] NewLengths { get; private set; }
        public int[] OldLengths { get; private set; }

        public IndexRange NewRange { get; private set; }
        public IndexRange OldRange { get; private set; }

        private ArrayChangedEventArgs()
        {
        }

        static public ArrayChangedEventArgs Replace(int[] startingIndexes, Array newValues, Array oldValues) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Replace,
            StartingIndexes = startingIndexes,
            NewValues = newValues,
            OldValues = oldValues,
            NewRange = new IndexRange(startingIndexes, newValues.Lengths()),
            OldRange = new IndexRange(startingIndexes, oldValues.Lengths())
        };

        static public ArrayChangedEventArgs Resize(int[] newLengths, int[] oldLengths) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Resize,
            NewLengths = newLengths,
            OldLengths = oldLengths
        };

        static public ArrayChangedEventArgs Add(int sliceDimension, int startingIndex, Array newValues, int[] oldLengths) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Add,
            StartingIndexes = GetStartingIndexes(oldLengths.Length, sliceDimension, startingIndex),
            NewValues = newValues,
            SliceDimension = sliceDimension,
            SliceCount = newValues.GetLength(sliceDimension),
            NewLengths = GetNewLengths(oldLengths, sliceDimension, newValues.GetLength(sliceDimension)),
            OldLengths = oldLengths,
            NewRange = GetSliceRange(oldLengths, startingIndex, sliceDimension, newValues.GetLength(sliceDimension))
        };

        static public ArrayChangedEventArgs Add(int sliceDimension, int startingIndex, int sliceCount, int[] oldLengths) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Add,
            StartingIndexes = GetStartingIndexes(oldLengths.Length, sliceDimension, startingIndex),
            SliceDimension = sliceDimension,
            SliceCount = sliceCount,
            NewLengths = GetNewLengths(oldLengths, sliceDimension, sliceCount),
            OldLengths = oldLengths,
            NewRange = GetSliceRange(oldLengths, startingIndex, sliceDimension, sliceCount)
        };

        static public ArrayChangedEventArgs Remove(int sliceDimension, int startingIndex, Array oldValues, int[] oldLengths) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Remove,
            StartingIndexes = GetStartingIndexes(oldLengths.Length, sliceDimension, startingIndex),
            SliceDimension = sliceDimension,
            SliceCount = oldValues.GetLength(sliceDimension),
            OldValues = oldValues,
            NewLengths = GetNewLengths(oldLengths, sliceDimension, -oldValues.GetLength(sliceDimension)),
            OldLengths = oldLengths,
            OldRange = GetSliceRange(oldLengths, startingIndex, sliceDimension, oldValues.GetLength(sliceDimension))
        };

        static public ArrayChangedEventArgs Remove(int sliceDimension, int startingIndex, int sliceCount, int[] oldLengths) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Remove,
            StartingIndexes = GetStartingIndexes(oldLengths.Length, sliceDimension, startingIndex),
            SliceDimension = sliceDimension,
            SliceCount = sliceCount,
            NewLengths = GetNewLengths(oldLengths, sliceDimension, -sliceCount),
            OldLengths = oldLengths,
            OldRange = GetSliceRange(oldLengths, startingIndex, sliceDimension, sliceCount)
        };

        static public ArrayChangedEventArgs Move(int arrayRank, int sliceDimension, int oldStartingIndex, int startingIndex, Array values) => new ArrayChangedEventArgs
        {
            Action = ArrayChangedAction.Move,
            StartingIndexes = GetStartingIndexes(arrayRank, sliceDimension, startingIndex),
            OldStartingIndexes = GetStartingIndexes(arrayRank, sliceDimension, oldStartingIndex),
            SliceDimension = sliceDimension,
            SliceCount = values.GetLength(sliceDimension),
            NewValues = values,
            NewRange = new IndexRange(GetStartingIndexes(arrayRank, sliceDimension, startingIndex), values.Lengths()),
            OldRange = new IndexRange(GetStartingIndexes(arrayRank, sliceDimension, oldStartingIndex), values.Lengths())
        };

        static private int[] GetStartingIndexes(int arrayRank, int sliceDimension, int startingIndex)
        {
            var startingIndexes = new int[arrayRank];
            startingIndexes[sliceDimension] = startingIndex;
            return startingIndexes;
        }

        static private int[] GetNewLengths(int[] oldLengths, int sliceDimension, int sliceCount)
        {
            var newLengths = new int[oldLengths.Length];
            Array.Copy(oldLengths, newLengths, oldLengths.Length);

            newLengths[sliceDimension] += sliceCount;
            return newLengths;
        }

        static private int[] GetRangeLengths(int[] oldLengths, int sliceDimension, int sliceCount)
        {
            var rangeLengths = new int[oldLengths.Length];
            Array.Copy(oldLengths, rangeLengths, oldLengths.Length);

            rangeLengths[sliceDimension] = sliceCount;
            return rangeLengths;
        }

        static private IndexRange GetSliceRange(int[] oldLengths, int startingIndex, int sliceDimension, int sliceCount)
        {
            return new IndexRange(GetStartingIndexes(oldLengths.Length, sliceDimension, startingIndex), GetRangeLengths(oldLengths, sliceDimension, sliceCount));
        }
    }
}
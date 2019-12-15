using System;

namespace Simulacra.Utils
{
    public class ArrayChangedEventArgs : EventArgs
    {
        public int[] StartingIndexes { get; set; }
        public Array NewValues { get; set; }
    }
}
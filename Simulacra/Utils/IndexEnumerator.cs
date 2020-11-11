using System.Collections;
using System.Collections.Generic;

namespace Simulacra.Utils
{
    public class IndexEnumerator : IEnumerator<int[]>
    {
        private readonly IArrayDefinition _array;

        public int[] Current { get; }
        object IEnumerator.Current => Current;

        public IndexEnumerator(IArrayDefinition array)
        {
            _array = array;
            Current = _array.GetResetIndex();
        }

        public void Reset() => _array.GetResetIndex(Current);
        public bool MoveNext() => _array.MoveIndex(Current);
        public void Dispose() { }
    }
}
using System.Collections;
using System.Collections.Generic;

namespace Simulacra.Utils
{
    public class IndexEnumerable : IEnumerable<int[]>
    {
        private readonly IArrayDefinition _array;
        public IndexEnumerable(IArrayDefinition array) => _array = array;

        public IEnumerator<int[]> GetEnumerator() => new IndexEnumerator(_array);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
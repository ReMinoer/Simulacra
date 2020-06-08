using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    public class RetypedArray<TOldValue, TNewValue> : IArray<TNewValue>
    {
        protected readonly IArray<TOldValue> Array;
        protected readonly Func<TOldValue, TNewValue> Getter;

        public RetypedArray(IArray<TOldValue> array, Func<TOldValue, TNewValue> getter)
        {
            Array = array;
            Getter = getter;
        }
            
        public int Rank => Array.Rank;
        public int GetLength(int dimension) => Array.GetLength(dimension);
        public TNewValue this[params int[] indexes] => Getter(Array[indexes]);
        object IArray.this[params int[] indexes] => this[indexes];

        public IEnumerator<TNewValue> GetEnumerator() => Array.Select(Getter).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
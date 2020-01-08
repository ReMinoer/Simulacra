using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Utils
{
    public class Array<T> : IWriteableArray<T>, INotifyArrayChanged
    {
        private readonly Array _array;
        private readonly Func<int[], T> _getter;
        private readonly Action<int[], T> _setter;

        public int Rank => _array.Rank;
        public int GetLength(int dimension) => _array.GetLength(dimension);

        public event EventHandler<ArrayChangedEventArgs> ArrayChanged;

        public Array(T[] array)
            : this(array, i => array[i[0]], (i, x) => array[i[0]] = x)
        {
        }

        public Array(T[][] array)
            : this(array, i => array[i[0]][i[1]], (i, x) => array[i[0]][i[1]] = x)
        {
        }

        public Array(T[][][] array)
            : this(array, i => array[i[0]][i[1]][i[2]], (i, x) => array[i[0]][i[1]][i[2]] = x)
        {
        }

        public Array(T[][][][] array)
            : this(array, i => array[i[0]][i[1]][i[2]][i[3]], (i, x) => array[i[0]][i[1]][i[2]][i[3]] = x)
        {
        }

        public Array(T[,] array)
            : this(array, i => array[i[0],i[1]], (i, x) => array[i[0],i[1]] = x)
        {
        }

        public Array(T[,,] array)
            : this(array, i => array[i[0],i[1],i[2]], (i, x) => array[i[0],i[1],i[2]] = x)
        {
        }

        public Array(T[,,,] array)
            : this(array, i => array[i[0],i[1],i[2],i[3]], (i, x) => array[i[0],i[1],i[2],i[3]] = x)
        {
        }

        public Array(Array array)
            : this(array, i => (T)array.GetValue(i), (i, x) => array.SetValue(x, i))
        {
        }

        private Array(Array array, Func<int[], T> getter, Action<int[], T> setter)
        {
            _array = array;
            _getter = getter;
            _setter = setter;
        }

        public T this[params int[] indexes]
        {
            get => _getter(indexes);
            set
            {
                if (EqualityComparer<T>.Default.Equals(_getter(indexes), value))
                    return;

                _setter(indexes, value);
                if (ArrayChanged == null)
                    return;

                Array newValue = Array.CreateInstance(typeof(T), Enumerable.Repeat(1, Rank).ToArray());
                newValue.SetValue(value, Enumerable.Repeat(0, Rank).ToArray());

                ArrayChanged?.Invoke(this, new ArrayChangedEventArgs
                {
                    StartingIndexes = indexes,
                    NewValues = newValue,
                });

            }
        }

        T IArray<T>.this[params int[] indexes] => _getter(indexes);

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private readonly Array<T> _array;
            private int[] _indexes;

            public Enumerator(Array<T> array)
            {
                _array = array;
                Reset();
            }

            public T Current => _array._getter(_indexes);
            object IEnumerator.Current => Current;
            
            public void Reset() => _indexes = new int[_array.Rank];
            public bool MoveNext() => _array.MoveToNextIndex(_indexes);

            public void Dispose()
            {
            }
        }
    }
}
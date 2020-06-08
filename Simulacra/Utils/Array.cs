using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Simulacra.Utils
{
    public class UnknownDimensionArray<T> : ArrayBase<T>
    {
        public UnknownDimensionArray(Array array)
            : base(array, i => (T)array.GetValue(i), (i, x) => array.SetValue(x, i))
        {
        }
    }

    public class OneDimensionArray<T> : ArrayBase<T>, IOneDimensionWriteableArray<T>, IList, INotifyCollectionChanged
    {
        private readonly T[] _array;

        public OneDimensionArray()
            : this(new T[0])
        {
        }

        public OneDimensionArray(int length)
            : this(new T[length])
        {
        }

        public OneDimensionArray(T[] array)
            : base(array, i => array[i[0]], (i, x) => array[i[0]] = x)
        {
            _array = array;
        }

        public T this[int i]
        {
            get => _array[i];
            set => SetValue(() => _array[i], x => _array[i] = x, value, () => new []{i});
        }

        object IOneDimensionArray.this[int i] => this[i];

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnSetValue(T value, T oldValue, int[] indexes)
        {
            base.OnSetValue(value, oldValue, indexes);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, indexes[0]));
        }

        bool ICollection.IsSynchronized => false;
        int ICollection.Count => this.Lengths().Sum();
        object ICollection.SyncRoot => _array;
        void ICollection.CopyTo(Array array, int index) => _array.CopyTo(array, index);

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => false;
        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void IList.Clear() => Array.Clear(_array, _array.GetLowerBound(0), GetLength(0));
        bool IList.Contains(object value) => Array.IndexOf(_array, value) >= _array.GetLowerBound(0);
        int IList.IndexOf(object value) => Array.IndexOf(_array, value);
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }
    }

    public class TwoDimensionArray<T> : ArrayBase<T>, ITwoDimensionWriteableArray<T>
    {
        private readonly Func<int, int, T> _getter;
        private readonly Action<int, int, T> _setter;

        public TwoDimensionArray()
            : this(new T[0, 0])
        {
        }

        public TwoDimensionArray(int length0, int length1)
            : this(new T[length0, length1])
        {
        }

        public TwoDimensionArray(T[][] array)
            : base(array, i => array[i[0]][i[1]], (i, x) => array[i[0]][i[1]] = x)
        {
            _getter = (i, j) => array[i][j];
            _setter = (i, j, value) => array[i][j] = value;
        }

        public TwoDimensionArray(T[,] array)
            : base(array, i => array[i[0],i[1]], (i, x) => array[i[0],i[1]] = x)
        {
            _getter = (i, j) => array[i, j];
            _setter = (i, j, value) => array[i, j] = value;
        }

        public T this[int i, int j]
        {
            get => _getter(i, j);
            set => SetValue(() => _getter(i, j), x => _setter(i, j, x), value, () => new []{i, j});
        }

        object ITwoDimensionArray.this[int i, int j] => this[i, j];
    }

    public class ThreeDimensionArray<T> : ArrayBase<T>, IThreeDimensionWriteableArray<T>
    {
        private readonly Func<int, int, int, T> _getter;
        private readonly Action<int, int, int, T> _setter;

        public ThreeDimensionArray()
            : this(new T[0, 0, 0])
        {
        }

        public ThreeDimensionArray(int length0, int length1, int length2)
            : this(new T[length0, length1, length2])
        {
        }

        public ThreeDimensionArray(T[][][] array)
            : base(array, i => array[i[0]][i[1]][i[2]], (i, x) => array[i[0]][i[1]][i[2]] = x)
        {
            _getter = (i, j, k) => array[i][j][k];
            _setter = (i, j, k, value) => array[i][j][k] = value;
        }

        public ThreeDimensionArray(T[,,] array)
            : base(array, i => array[i[0],i[1],i[2]], (i, x) => array[i[0],i[1],i[2]] = x)
        {
            _getter = (i, j, k) => array[i, j, k];
            _setter = (i, j, k, value) => array[i, j, k] = value;
        }

        public T this[int i, int j, int k]
        {
            get => _getter(i, j, k);
            set => SetValue(() => _getter(i, j, k), x => _setter(i, j, k, x), value, () => new []{i, j, k});
        }

        object IThreeDimensionArray.this[int i, int j, int k] => this[i, j, k];
    }

    public class FourDimensionArray<T> : ArrayBase<T>, IFourDimensionWriteableArray<T>
    {
        private readonly Func<int, int, int, int, T> _getter;
        private readonly Action<int, int, int, int, T> _setter;

        public FourDimensionArray()
            : this(new T[0, 0, 0, 0])
        {
        }

        public FourDimensionArray(int length0, int length1, int length2, int length3)
            : this(new T[length0, length1, length2, length3])
        {
        }

        public FourDimensionArray(T[][][][] array)
            : base(array, i => array[i[0]][i[1]][i[2]][i[3]], (i, x) => array[i[0]][i[1]][i[2]][i[3]] = x)
        {
            _getter = (i, j, k, l) => array[i][j][k][l];
            _setter = (i, j, k, l, value) => array[i][j][k][l] = value;
        }

        public FourDimensionArray(T[,,,] array)
            : base(array, i => array[i[0],i[1],i[2],i[3]], (i, x) => array[i[0],i[1],i[2],i[3]] = x)
        {
            _getter = (i, j, k, l) => array[i, j, k, l];
            _setter = (i, j, k, l, value) => array[i, j, k, l] = value;
        }

        public T this[int i, int j, int k, int l]
        {
            get => _getter(i, j, k, l);
            set => SetValue(() => _getter(i, j, k, l), x => _setter(i, j, k, l, x), value, () => new []{i, j, k, l});
        }

        object IFourDimensionArray.this[int i, int j, int k, int l] => this[i, j, k, l];
    }

    public abstract class ArrayBase<T> : IWriteableArray<T>, INotifyArrayChanged, IStructuralComparable, IStructuralEquatable
    {
        private readonly Array _data;
        private readonly Func<int[], T> _getter;
        private readonly Action<int[], T> _setter;

        public int Rank => _data.Rank;
        public int GetLength(int dimension) => _data.GetLength(dimension);

        public event ArrayChangedEventHandler ArrayChanged;

        protected ArrayBase(Array data, Func<int[], T> getter, Action<int[], T> setter)
        {
            _data = data;
            _getter = getter;
            _setter = setter;
        }

        public T this[params int[] indexes]
        {
            get => _getter(indexes);
            set => SetValue(() => _getter(indexes), x => _setter(indexes, x), value, () => indexes);
        }

        protected void SetValue(Func<T> getter, Action<T> setter, T value, Func<int[]> eventIndexesFunc)
        {
            T oldValue = getter();
            if (EqualityComparer<T>.Default.Equals(oldValue, value))
                return;

            setter(value);

            int[] indexes = eventIndexesFunc();
            OnSetValue(value, oldValue, indexes);
        }

        protected virtual void OnSetValue(T value, T oldValue, int[] indexes)
        {
            Array newValues = Array.CreateInstance(typeof(T), Enumerable.Repeat(1, Rank).ToArray());
            newValues.SetValue(value, Enumerable.Repeat(0, Rank).ToArray());

            ArrayChanged?.Invoke(this, new ArrayChangedEventArgs { StartingIndexes = indexes, NewValues = newValues });
        }

        T IArray<T>.this[params int[] indexes] => _getter(indexes);
        object IArray.this[params int[] indexes] => _getter(indexes);

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private readonly ArrayBase<T> _array;
            private int[] _indexes;

            public Enumerator(ArrayBase<T> array)
            {
                _array = array;
                Reset();
            }

            public T Current => _array._getter(_indexes);
            object IEnumerator.Current => Current;

            public void Reset() => _indexes = _array.GetResetIndex();
            public bool MoveNext() => _array.MoveIndex(_indexes);

            public void Dispose()
            {
            }
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer) => ((IStructuralComparable)_data).CompareTo(other, comparer);
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)_data).Equals(other, comparer);
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)_data).GetHashCode(comparer);
    }
}
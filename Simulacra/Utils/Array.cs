using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Enumerable = System.Linq.Enumerable;
// Do not using System.Linq for performance

namespace Simulacra.Utils
{
    public class UnknownDimensionArray<T> : ArrayBase<Array, T>
    {
        public override int Rank => Data.Rank;

        public UnknownDimensionArray(Array array, int[] lengths = null)
            : base(array, lengths ?? array.Lengths(), array.Lengths())
        {
        }

        protected override sealed T Get(Array data, int[] indexes) => (T)data.GetValue(indexes);
        protected override sealed void Set(Array data, int[] indexes, T value) => data.SetValue(value, indexes);
        protected override Array CreateValueArray(T value)
        {
            var lengthOne = new int[Rank];
            var indexesZero = new int[Rank];
            for (int i = 0; i < Rank; i++)
            {
                lengthOne[i] = 1;
                indexesZero[i] = 0;
            }

            var newValues = Array.CreateInstance(typeof(T), lengthOne);
            newValues.SetValue(value, indexesZero);
            return newValues;
        }

        protected override Array ResizeData(Array data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory)
        {
            return data.ToResizedArray(capacities, keepValues, valueFactory, lengths);
        }

        protected override void FillData(Array data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            data.Fill(valueFactory, indexEnumerator, excludingMasks);
        }
    }

    public class OneDimensionArray<T> : ArrayBase<T[], T>, IOneDimensionResizeableArray<T>, IList, INotifyCollectionChanged
    {
        public override int Rank => 1;

        public OneDimensionArray()
            : this(new T[0])
        {
        }

        public OneDimensionArray(int length0, int[] capacity = null)
            : this(capacity != null ? new T[capacity[0]] : new T[length0], new[] { length0 })
        {
        }

        public OneDimensionArray(T[] array, int[] lengths = null)
            : base(array, lengths ?? array.Lengths(), array.Lengths())
        {
        }

        public T this[int i]
        {
            get => GetValue(a => a[i], x => IndexSwitch(x, i));
            set => SetValue(a => a[i], (a, x) => a[i] = x, value, x => IndexSwitch(x, i), () => new[] { i });
        }

        object IOneDimensionArray.this[int i] => this[i];

        protected override sealed T Get(T[] data, int[] indexes) => data[indexes[0]];
        protected override sealed void Set(T[] data, int[] indexes, T value) => data[indexes[0]] = value;
        protected override sealed Array CreateValueArray(T value) => new[] { value };

        protected override T[] ResizeData(T[] data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory)
        {
            return (T[])data.ToResizedArray(capacities, keepValues, valueFactory, lengths);
        }

        protected override void FillData(T[] data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            data.Fill(valueFactory, indexEnumerator, excludingMasks);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnSetValue(T value, T oldValue, Func<int, int> indexesFunc, Func<int[]> indexesArrayFunc)
        {
            base.OnSetValue(value, oldValue, indexesFunc, indexesArrayFunc);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, indexesFunc(0)));
        }

        bool ICollection.IsSynchronized => false;
        int ICollection.Count => Data.Length;
        object ICollection.SyncRoot => Data;
        void ICollection.CopyTo(Array array, int index) => Data.CopyTo(array, index);

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => false;
        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void IList.Clear() => Array.Clear(Data, Data.GetLowerBound(0), GetLength(0));
        bool IList.Contains(object value) => Array.IndexOf(Data, value) >= Data.GetLowerBound(0);
        int IList.IndexOf(object value) => Array.IndexOf(Data, value);
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }
    }

    public class TwoDimensionArray<T> : ArrayBase<T[,], T>, ITwoDimensionResizeableArray<T>
    {
        public override int Rank => 2;

        public TwoDimensionArray()
            : this(new T[0, 0])
        {
        }

        public TwoDimensionArray(int length0, int length1, int[] capacity = null)
            : this(capacity != null ? new T[capacity[0], capacity[1]] : new T[length0, length1], new[] { length0, length1 })
        {
        }

        public TwoDimensionArray(T[,] array, int[] lengths = null)
            : base(array, lengths ?? array.Lengths(), array.Lengths())
        {
        }

        public T this[int i, int j]
        {
            get => GetValue(a => a[i, j], x => IndexSwitch(x, i, j));
            set => SetValue(a => a[i, j], (a, x) => a[i, j] = x, value, x => IndexSwitch(x, i, j), () => new[] { i, j });
        }

        object ITwoDimensionArray.this[int i, int j] => this[i, j];

        protected override sealed T Get(T[,] data, int[] indexes) => data[indexes[0], indexes[1]];
        protected override sealed void Set(T[,] data, int[] indexes, T value) => data[indexes[0], indexes[1]] = value;
        protected override sealed Array CreateValueArray(T value) => new[,] { { value } };

        protected override T[,] ResizeData(T[,] data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory)
        {
            return (T[,])data.ToResizedArray(capacities, keepValues, valueFactory, lengths);
        }

        protected override void FillData(T[,] data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            data.Fill(valueFactory, indexEnumerator, excludingMasks);
        }
    }

    public class ThreeDimensionArray<T> : ArrayBase<T[,,], T>, IThreeDimensionResizeableArray<T>
    {
        public override int Rank => 3;

        public ThreeDimensionArray()
            : this(new T[0, 0, 0])
        {
        }

        public ThreeDimensionArray(int length0, int length1, int length2, int[] capacity = null)
            : this(capacity != null ? new T[capacity[0], capacity[1], capacity[2]] : new T[length0, length1, length2], new[] { length0, length1, length2 })
        {
        }

        public ThreeDimensionArray(T[,,] array, int[] lengths = null)
            : base(array, lengths ?? array.Lengths(), array.Lengths())
        {
        }

        public T this[int i, int j, int k]
        {
            get => GetValue(a => a[i, j, k], x => IndexSwitch(x, i, j, k));
            set => SetValue(a => a[i, j, k], (a, x) => a[i, j, k] = x, value, x => IndexSwitch(x, i, j, k), () => new[] { i, j, k });
        }

        object IThreeDimensionArray.this[int i, int j, int k] => this[i, j, k];

        protected override sealed T Get(T[,,] data, int[] indexes) => data[indexes[0], indexes[1], indexes[2]];
        protected override sealed void Set(T[,,] data, int[] indexes, T value) => data[indexes[0], indexes[1], indexes[2]] = value;
        protected override sealed Array CreateValueArray(T value) => new[,,] { { { value } } };

        protected override T[,,] ResizeData(T[,,] data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory)
        {
            return (T[,,])data.ToResizedArray(capacities, keepValues, valueFactory, lengths);
        }

        protected override void FillData(T[,,] data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            data.Fill(valueFactory, indexEnumerator, excludingMasks);
        }
    }

    public class FourDimensionArray<T> : ArrayBase<T[,,,], T>, IFourDimensionResizeableArray<T>
    {
        public override int Rank => 4;

        public FourDimensionArray()
            : this(new T[0, 0, 0, 0])
        {
        }

        public FourDimensionArray(int length0, int length1, int length2, int length3, int[] capacity = null)
            : this(capacity != null ? new T[capacity[0], capacity[1], capacity[2], capacity[3]] : new T[length0, length1, length2, length3], new []{ length0, length1, length2, length3 })
        {
        }

        public FourDimensionArray(T[,,,] array, int[] lengths = null)
            : base(array, lengths ?? array.Lengths(), array.Lengths())
        {
        }

        public T this[int i, int j, int k, int l]
        {
            get => GetValue(a => a[i, j, k, l], x => IndexSwitch(x, i, j, k, l));
            set => SetValue(a => a[i, j, k, l], (a, x) => a[i, j, k, l] = x, value, x => IndexSwitch(x, i, j, k, l), () => new []{i, j, k, l});
        }

        object IFourDimensionArray.this[int i, int j, int k, int l] => this[i, j, k, l];

        protected override sealed T Get(T[,,,] data, int[] indexes) => data[indexes[0], indexes[1], indexes[2], indexes[3]];
        protected override sealed void Set(T[,,,] data, int[] indexes, T value) => data[indexes[0], indexes[1], indexes[2], indexes[3]] = value;
        protected override sealed Array CreateValueArray(T value) => new[,,,] { { { { value } } } };

        protected override T[,,,] ResizeData(T[,,,] data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory)
        {
            return (T[,,,])data.ToResizedArray(capacities, keepValues, valueFactory, lengths);
        }

        protected override void FillData(T[,,,] data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks)
        {
            data.Fill(valueFactory, indexEnumerator, excludingMasks);
        }
    }

    public abstract class ArrayBase<TData, T> : IResizeableArray<T>, INotifyArrayChanged, INotifyPropertyChanged, IStructuralComparable, IStructuralEquatable
        where TData : class, IStructuralComparable
    {
        protected TData Data;

        public abstract int Rank { get; }
        public int GetLength(int dimension) => _lengths[dimension];

        private readonly int[] _lengths;
        public int[] Lengths
        {
            get => _lengths;
            set
            {
                if (Enumerable.SequenceEqual(_lengths, value))
                    return;

                Resize(value, keepValues: true);
            }
        }

        private readonly int[] _capacities;
        public int[] Capacities
        {
            get => _capacities; 
            set
            {
                if (Enumerable.SequenceEqual(_capacities, value))
                    return;

                if (NeedDecreaseLengths(value, out int[] newLengths))
                {
                    int[] oldLengths = Enumerable.ToArray(_lengths);

                    Array.Copy(value, _capacities, value.Length);
                    Array.Copy(newLengths, _lengths, newLengths.Length);

                    Data = ResizeData(Data, Capacities, Lengths, keepValues: true, valueFactory: null);

                    NotifyPropertyChanged(nameof(Capacities));
                    NotifyPropertyChanged(nameof(Lengths));
                    ArrayChanged?.Invoke(this, ArrayChangedEventArgs.Resize(Lengths, oldLengths));
                }
                else
                {
                    Array.Copy(value, _capacities, value.Length);

                    Data = ResizeData(Data, Capacities, Lengths, keepValues: true, valueFactory: null);

                    NotifyPropertyChanged(nameof(Capacities));
                }
            }
        }

        public event ArrayChangedEventHandler ArrayChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected ArrayBase(TData data, int[] lengths, int[] capacities)
        {
            Data = data;
            _lengths = lengths;
            _capacities = capacities;
        }

        public T this[params int[] indexes]
        {
            get => GetValue(a => Get(a, indexes), i => indexes[i]);
            set => SetValue(a => Get(a, indexes), (a, x) => Set(a, indexes, x), value, i => indexes[i], () => indexes);
        }

        public void Resize(int[] newLengths, bool keepValues = true, Func<T, int[], T> valueFactory = null)
        {
            int[] oldLengths = Enumerable.ToArray(_lengths);

            if (NeedIncreaseCapacities(newLengths, out int[] newCapacities))
            {
                Array.Copy(newLengths, _lengths, newLengths.Length);
                Array.Copy(newCapacities, _capacities, newCapacities.Length);

                Data = ResizeData(Data, _capacities, _lengths, keepValues, valueFactory);

                NotifyPropertyChanged(nameof(Capacities));
            }
            else
            {
                Array.Copy(newLengths, _lengths, newLengths.Length);
                FillData(Data, valueFactory ?? ((_, __) => default), new IndexRange(_lengths), new IArrayMask[] { new IndexRange(oldLengths) });
            }

            NotifyPropertyChanged(nameof(Lengths));
            ArrayChanged?.Invoke(this, ArrayChangedEventArgs.Resize(Lengths, oldLengths));
        }

        private bool NeedIncreaseCapacities(int[] newLengths, out int[] newCapacities)
        {
            newCapacities = Enumerable.ToArray(Enumerable.Zip(newLengths, Capacities, Math.Max));

            for (int i = 0; i < Rank; i++)
                if (newCapacities[i] > Capacities[i])
                    return true;

            return false;
        }

        private bool NeedDecreaseLengths(int[] newCapacities, out int[] newLengths)
        {
            newLengths = Enumerable.ToArray(Enumerable.Zip(Lengths, newCapacities, Math.Min));

            for (int i = 0; i < Rank; i++)
                if (newLengths[i] < Lengths[i])
                    return true;

            return false;
        }

        protected T GetValue(Func<TData, T> getter, Func<int, int> indexesFunc)
        {
            for (int r = 0; r < Rank; r++)
            {
                int index = indexesFunc(r);
                if (index < 0 && index >= GetLength(r))
                    throw new IndexOutOfRangeException();
            }

            return getter(Data);
        }

        protected void SetValue(Func<TData, T> getter, Action<TData, T> setter, T value, Func<int, int> indexesFunc, Func<int[]> indexesArrayFunc)
        {
            T previousValue = GetValue(getter, indexesFunc);
            if (EqualityComparer<T>.Default.Equals(previousValue, value))
                return;

            setter(Data, value);
            OnSetValue(value, previousValue, indexesFunc, indexesArrayFunc);
        }

        protected virtual void OnSetValue(T value, T oldValue, Func<int, int> indexesFunc, Func<int[]> indexesArrayFunc)
        {
            if (ArrayChanged == null)
                return;

            int[] indexes = indexesArrayFunc?.Invoke();
            if (indexes == null)
            {
                indexes = new int[Rank];
                for (int i = 0; i < indexes.Length; i++)
                    indexes[i] = indexesFunc(i);
            }

            ArrayChanged?.Invoke(this, ArrayChangedEventArgs.Replace(indexes, CreateValueArray(value), CreateValueArray(oldValue)));
        }

        static protected int IndexSwitch(int rank, int i = -1, int j = -1, int k = -1, int l = -1)
        {
            switch (rank)
            {
                case 0: return i;
                case 1: return j;
                case 2: return k;
                case 3: return l;
                default: throw new NotSupportedException();
            }
        }

        protected abstract T Get(TData data, int[] indexes);
        protected abstract void Set(TData data, int[] indexes, T value);

        protected abstract TData ResizeData(TData data, int[] capacities, int[] lengths, bool keepValues, Func<T, int[], T> valueFactory);
        protected abstract void FillData(TData data, Func<T, int[], T> valueFactory, IIndexEnumerator indexEnumerator, IArrayMask[] excludingMasks);
        protected abstract Array CreateValueArray(T value);

        T IArray<T>.this[params int[] indexes] => Get(Data, indexes);
        object IArray.this[params int[] indexes] => Get(Data, indexes);

        int IStructuralComparable.CompareTo(object other, IComparer comparer) => Data.CompareTo(other, comparer);
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)Data).Equals(other, comparer);
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Data).GetHashCode(comparer);

        public int[] GetResetIndex() => ArrayUtils.GetResetIndex(this);
        public bool MoveIndex(int[] indexes) => ArrayUtils.MoveIndex(this, indexes);

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<T>
        {
            private readonly ArrayBase<TData, T> _array;
            private int[] _indexes;

            public Enumerator(ArrayBase<TData, T> array)
            {
                _array = array;
                Reset();
            }

            public T Current => _array.Get(_array.Data, _indexes);
            object IEnumerator.Current => Current;

            public void Reset() => _indexes = _array.GetResetIndex();
            public bool MoveNext() => _array.MoveIndex(_indexes);

            public void Dispose()
            {
            }
        }
    }
}
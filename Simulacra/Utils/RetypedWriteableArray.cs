using System;

namespace Simulacra.Utils
{
    public class RetypedWriteableArray<TOldValue, TNewValue> : RetypedArray<TOldValue, TNewValue>, IWriteableArray<TNewValue>
    {
        protected readonly Action<TOldValue, TNewValue> Setter;

        public RetypedWriteableArray(IArray<TOldValue> array, Func<TOldValue, TNewValue> getter, Action<TOldValue, TNewValue> setter)
            : base(array, getter)
        {
            Setter = setter;
        }

        new public TNewValue this[params int[] indexes]
        {
            get => base[indexes];
            set => Setter(Array[indexes], value);
        }
    }
}
using System;
using System.Linq;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class OneWayArrayBinding<TModel, TView, TModelItem, TViewItem, TViewArray> : IOneWayBinding<TModel, TView, ArrayChangedEventArgs>
        where TViewArray : IWriteableArray<TViewItem>
    {
        private readonly Func<TModel, IArray<TModelItem>> _referenceGetter;
        private readonly Func<TView, IWriteableArray<TViewItem>> _arrayGetter;
        private readonly Action<TView, TViewArray> _arraySetter;
        private readonly Func<int[], TViewArray> _arrayCreator;
        private readonly Func<TModel, TModelItem, TView, TViewItem> _itemConverter;
        private readonly Action<TViewItem> _viewItemDisposer;

        public OneWayArrayBinding(Func<TModel, IArray<TModelItem>> referenceGetter, Func<TView, IWriteableArray<TViewItem>> arrayGetter, Action<TView, TViewArray> arraySetter, Func<int[], TViewArray> arrayCreator, Func<TModel, TModelItem, TView, TViewItem> itemConverter, Action<TViewItem> viewItemDisposer = null)
        {
            _referenceGetter = referenceGetter;
            _arrayGetter = arrayGetter;
            _arrayCreator = arrayCreator;
            _arraySetter = arraySetter;
            _itemConverter = itemConverter;
            _viewItemDisposer = viewItemDisposer;
        }

        public void SetView(TModel model, TView view)
        {
            IArray<TModelItem> referenceArray = _referenceGetter(model);
            IWriteableArray<TViewItem> array = _arrayGetter(view);

            if (array != null && referenceArray.Lengths().SequenceEqual(array.Lengths()))
            {
                int[] indexes = referenceArray.GetResetIndex();
                while (referenceArray.MoveIndex(indexes))
                    SetCellValue(model, view, array, indexes, referenceArray[indexes]);
            }
            else
            {
                TViewArray newArray = _arrayCreator(referenceArray.Lengths().ToArray());

                int[] indexes = referenceArray.GetResetIndex();
                while (referenceArray.MoveIndex(indexes))
                    SetCellValue(model, view, newArray, indexes, referenceArray[indexes]);

                _arraySetter(view, newArray);
            }
        }

        public void UpdateView(TModel model, TView view, ArrayChangedEventArgs e)
        {
            IWriteableArray<TViewItem> array = _arrayGetter(view);
            int[] arrayIndexes = e.StartingIndexes.ToArray();

            System.Array values = e.NewValues;
            var valueIndexes = e.NewValues.GetInitialIndex();
            
            while (true)
            {
                SetCellValue(model, view, array, arrayIndexes, (TModelItem)values.GetValue(valueIndexes));

                if (!values.MoveIndex(valueIndexes))
                    break;
                if (!array.MoveIndex(arrayIndexes))
                    throw new InvalidOperationException();
            }
        }

        private void SetCellValue(TModel model, TView view, IWriteableArray<TViewItem> array, int[] indexes, TModelItem modelItem)
        {
            _viewItemDisposer?.Invoke(array[indexes]);
            array[indexes] = _itemConverter(model, modelItem, view);
        }
    }
}
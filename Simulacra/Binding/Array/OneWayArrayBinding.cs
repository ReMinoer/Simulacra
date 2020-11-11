using System;
using System.Linq;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class OneWayArrayBinding<TModel, TView, TModelItem, TViewItem, TViewArray, TCreatedArray> : IOneWayBinding<TModel, TView, ArrayChangedEventArgs>
        where TViewArray : IWriteableArray<TViewItem>
        where TCreatedArray : TViewArray
    {
        private readonly Func<TModel, IArray<TModelItem>> _referenceGetter;
        private readonly Func<TView, TViewArray> _arrayGetter;
        private readonly Action<TView, TViewArray> _arraySetter;
        private readonly Action<TModel, TView, TViewArray, int[]> _arrayResizer;
        private readonly Func<TModel, TView, int[], TCreatedArray> _arrayCreator;
        private readonly Func<TModel, TModelItem, TView, TViewItem, TViewItem> _itemConverter;
        private readonly Action<TViewItem> _viewItemDisposer;

        public OneWayArrayBinding(
            Func<TModel, IArray<TModelItem>> referenceGetter,
            Func<TView, TViewArray> arrayGetter,
            Action<TView, TViewArray> arraySetter,
            Action<TModel, TView, TViewArray, int[]> arrayResizer,
            Func<TModel, TView, int[], TCreatedArray> arrayCreator,
            Func<TModel, TModelItem, TView, TViewItem, TViewItem> itemConverter,
            Action<TViewItem> viewItemDisposer = null)
        {
            _referenceGetter = referenceGetter;
            _arrayGetter = arrayGetter;
            _arrayResizer = arrayResizer;
            _arrayCreator = arrayCreator;
            _arraySetter = arraySetter;
            _itemConverter = itemConverter;
            _viewItemDisposer = viewItemDisposer;
        }

        public void SetView(TModel model, TView view)
        {
            IArray<TModelItem> modelArray = _referenceGetter(model);
            TViewArray viewArray = _arrayGetter(view);

            int[] newLengths = modelArray.Lengths();

            if (viewArray != null && viewArray.Lengths().SequenceEqual(newLengths))
            {
                SetViewCells(model, view, viewArray);
            }
            else if (viewArray != null && _arrayResizer != null)
            {
                _arrayResizer.Invoke(model, view, viewArray, newLengths);
                SetViewCells(model, view, viewArray);
            }
            else
            {
                viewArray = _arrayCreator(model, view, newLengths);
                SetViewCells(model, view, viewArray);
                _arraySetter(view, viewArray);
            }
        }

        public void UpdateView(TModel model, TView view, ArrayChangedEventArgs e)
        {
            switch (e.Action)
            {
                case ArrayChangedAction.Replace:
                {
                    SetViewCells(model, view, e.NewRange, e.NewValues);
                    return;
                }
                case ArrayChangedAction.Resize:
                {
                    ResizeViewArray(model, view, e.NewLengths);
                    return;
                }
                case ArrayChangedAction.Add:
                {
                    throw new NotImplementedException();
                }
                case ArrayChangedAction.Remove:
                {
                    throw new NotImplementedException();
                }
                case ArrayChangedAction.Move:
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void ResizeViewArray(TModel model, TView view, int[] newLengths)
        {
            TViewArray viewArray = _arrayGetter(view);
            if (viewArray.Lengths().SequenceEqual(newLengths))
                return;

            if (_arrayResizer != null)
            {
                _arrayResizer.Invoke(model, view, viewArray, newLengths);
            }
            else
            {
                TViewArray newArray = _arrayCreator(model, view, newLengths);
                SetViewCells(model, view, newArray);
                _arraySetter(view, newArray);
            }
        }

        private void SetViewCells(TModel model, TView view, TViewArray viewArray)
        {
            IArray<TModelItem> modelArray = _referenceGetter(model);

            int[] indexes = modelArray.GetResetIndex();
            while (modelArray.MoveIndex(indexes))
                SetViewCell(model, view, viewArray, indexes, modelArray[indexes]);
        }

        private void SetViewCells(TModel model, TView view, IndexRange indexRange, System.Array newValues)
        {
            IWriteableArray<TViewItem> array = _arrayGetter(view);

            int[] arrayIndexes = indexRange.GetResetIndex();
            IndexRange newValuesRange = newValues.IndexRange();
            int[] newValueIndexes = newValuesRange.GetResetIndex();

            while (newValuesRange.MoveIndex(newValueIndexes))
            {
                if (!indexRange.MoveIndex(arrayIndexes))
                    throw new InvalidOperationException();

                var newModelItem = (TModelItem)newValues.GetValue(newValueIndexes);
                SetViewCell(model, view, array, arrayIndexes, newModelItem);
            }

            if (indexRange.MoveIndex(arrayIndexes))
                throw new InvalidOperationException();
        }

        private void SetViewCell(TModel model, TView view, IWriteableArray<TViewItem> array, int[] arrayIndexes, TModelItem newModelItem)
        {
            TViewItem oldViewItem = array[arrayIndexes];
            _viewItemDisposer?.Invoke(oldViewItem);

            array[arrayIndexes] = _itemConverter(model, newModelItem, view, oldViewItem);
        }
    }
}
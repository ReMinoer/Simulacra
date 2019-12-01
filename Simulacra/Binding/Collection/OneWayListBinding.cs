using System;
using System.Collections.Generic;
using System.Linq;
using Simulacra.Binding.Collection.Base;
using Simulacra.Binding.Collection.Utils;

namespace Simulacra.Binding.Collection
{
    public class OneWayListBinding<TModel, TView, TModelItem, TViewItem> : OneWayListBindingBase<TModel, TView, TModelItem, TViewItem>
    {
        private readonly Func<TView, IList<TViewItem>> _listGetter;
        private readonly Func<TModel, TModelItem, TView, TViewItem> _itemConverter;
        private readonly Func<TModelItem, TViewItem, bool> _itemEquality;
        private readonly Action<TViewItem> _viewItemDisposer;

        public bool FullClear { get; set; }

        public OneWayListBinding(Func<TModel, IEnumerable<TModelItem>> referenceGetter, Func<TView, IList<TViewItem>> listGetter, Func<TModel, TModelItem, TView, TViewItem> itemConverter, Func<TModelItem, TViewItem, bool> itemEquality, Action<TViewItem> viewItemDisposer = null)
            : base(referenceGetter)
        {
            _listGetter = listGetter;
            _itemConverter = itemConverter;
            _itemEquality = itemEquality;
            _viewItemDisposer = viewItemDisposer;
        }

        protected override void ResetView(TModel model, TView view)
        {
            if (FullClear)
            {
                ICollection<TViewItem> clearedItems = _listGetter(view).ToArray();
                _listGetter(view).Clear();

                foreach (TViewItem clearedItem in clearedItems)
                    DisposeViewItem(view, clearedItem);
            }
            else
            {
                base.ResetView(model, view);
            }
        }

        protected override void AddViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem) => _listGetter(view).Add(viewItem);
        protected override void RemoveViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem) => _listGetter(view).Remove(viewItem);
        protected override void DisposeViewItem(TView view, TViewItem viewItem) => _viewItemDisposer?.Invoke(viewItem);
        protected override TViewItem CreateBindedViewItem(TView view, TModel model, TModelItem modelItem) => _itemConverter(model, modelItem, view);
        protected override TViewItem GetBindedViewItem(TView view, TModelItem modelItem) => _listGetter(view).First(x => _itemEquality(modelItem, x));

        protected override void InsertViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int index) => _listGetter(view).InsertMany(index, viewItems);
        protected override void ReplaceViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int oldIndex, int newIndex) => _listGetter(view).ReplaceRange(oldIndex, newIndex, viewItems);
        protected override void MoveViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int index) => _listGetter(view).MoveMany(viewItems, index);
    }
}
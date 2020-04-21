using System;
using System.Collections.Generic;
using System.Linq;
using Simulacra.Binding.Collection.Base;

namespace Simulacra.Binding.Collection
{
    public class OneWayCollectionBinding<TModel, TView, TModelItem, TViewItem> : OneWayCollectionBindingBase<TModel, TView, TModelItem, TViewItem>
    {
        private readonly Func<TView, ICollection<TViewItem>> _collectionGetter;
        private readonly Func<TModel, TModelItem, TView, TViewItem> _itemConverter;
        private readonly Func<TModelItem, TViewItem, bool> _itemEquality;
        private readonly Action<TViewItem> _viewItemDisposer;

        public bool FullClear { get; set; }

        public OneWayCollectionBinding(Func<TModel, IEnumerable<TModelItem>> referenceGetter, Func<TView, ICollection<TViewItem>> collectionGetter, Func<TModel, TModelItem, TView, TViewItem> itemConverter, Func<TModelItem, TViewItem, bool> itemEquality, Action<TViewItem> viewItemDisposer = null)
            : base(referenceGetter)
        {
            _collectionGetter = collectionGetter;
            _itemConverter = itemConverter;
            _itemEquality = itemEquality;
            _viewItemDisposer = viewItemDisposer;
        }

        protected override void ResetView(TModel model, TView view)
        {
            if (FullClear)
            {
                ICollection<TViewItem> clearedItems = _collectionGetter(view).ToArray();
                _collectionGetter(view).Clear();

                foreach (TViewItem clearedItem in clearedItems)
                    DisposeViewItem(view, clearedItem);
            }
            else
            {
                base.ResetView(model, view);
            }
        }

        protected override void AddViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem) => _collectionGetter(view).Add(viewItem);
        protected override void RemoveViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem) => _collectionGetter(view).Remove(viewItem);
        protected override void DisposeViewItem(TView view, TViewItem viewItem) => _viewItemDisposer?.Invoke(viewItem);
        protected override TViewItem CreateBindedViewItem(TView view, TModel model, TModelItem modelItem) => _itemConverter(model, modelItem, view);
        protected override TViewItem GetBindedViewItem(TView view, TModel model, TModelItem modelItem) => _collectionGetter(view).First(x => _itemEquality(modelItem, x));
    }
}
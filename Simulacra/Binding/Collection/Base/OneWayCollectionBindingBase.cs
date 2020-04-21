using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Simulacra.Binding.Collection.Base
{
    public abstract class OneWayCollectionBindingBase<TModel, TView, TModelItem, TViewItem> : IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>
    {
        private readonly Func<TModel, IEnumerable<TModelItem>> _referenceGetter;

        protected OneWayCollectionBindingBase(Func<TModel, IEnumerable<TModelItem>> referenceGetter)
        {
            _referenceGetter = referenceGetter;
        }

        public void SetView(TModel model, TView view)
        {
            AddItems(model, _referenceGetter(model), view);
        }

        protected virtual void ResetView(TModel model, TView view)
        {
            RemoveItems(model, _referenceGetter(model), view);
        }

        public virtual void UpdateView(TModel model, TView view, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    AddItems(model, e.NewItems.Cast<TModelItem>(), view);
                    return;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    RemoveItems(model, e.OldItems.Cast<TModelItem>(), view);
                    return;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    RemoveItems(model, e.OldItems.Cast<TModelItem>(), view);
                    AddItems(model, e.NewItems.Cast<TModelItem>(), view);
                    return;
                }
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                {
                    ResetView(model, view);
                    SetView(model, view);
                    return;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        protected abstract void AddViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem);
        protected abstract void RemoveViewItem(TView view, TViewItem viewItem, TModel model, TModelItem modelItem);
        protected abstract void DisposeViewItem(TView view, TViewItem viewItem);
        protected abstract TViewItem CreateBindedViewItem(TView view, TModel model, TModelItem modelItem);
        protected abstract TViewItem GetBindedViewItem(TView view, TModel model, TModelItem modelItem);

        private void AddItems(TModel model, IEnumerable<TModelItem> modelItems, TView view)
        {
            foreach (TModelItem modelItem in modelItems)
            {
                TViewItem viewItem = CreateBindedViewItem(view, model, modelItem);
                AddViewItem(view, viewItem, model, modelItem);
            }
        }

        private void RemoveItems(TModel model, IEnumerable<TModelItem> modelItems, TView view)
        {
            foreach (TModelItem modelItem in modelItems)
            {
                TViewItem viewItem = GetBindedViewItem(view, model, modelItem);
                RemoveViewItem(view, viewItem, model, modelItem);
                DisposeViewItem(view, viewItem);
            }
        }
    }
}
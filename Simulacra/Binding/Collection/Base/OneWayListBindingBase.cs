using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Simulacra.Binding.Collection.Base
{
    public abstract class OneWayListBindingBase<TModel, TView, TModelItem, TViewItem> : OneWayCollectionBindingBase<TModel, TView, TModelItem, TViewItem>
    {
        protected OneWayListBindingBase(Func<TModel, IEnumerable<TModelItem>> referenceGetter)
            : base(referenceGetter)
        {
        }

        public override void UpdateView(TModel model, TView view, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    InsertItems(model, e.NewItems.Cast<TModelItem>(), e.NewStartingIndex, view);
                    return;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    ReplaceItems(model, e.NewItems.Cast<TModelItem>(), e.NewStartingIndex, view);
                    return;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    MoveItems(model, e.NewItems.Cast<TModelItem>(), e.NewStartingIndex, view);
                    return;
                }
                default:
                {
                    base.UpdateView(model, view, e);
                    return;
                }
            }
        }

        protected abstract void InsertViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int index);
        protected abstract void ReplaceViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int index);
        protected abstract void MoveViewItems(TView view, IEnumerable<TViewItem> viewItems, TModel model, IEnumerable<TModelItem> modelItems, int index);

        protected void InsertItems(TModel model, IEnumerable<TModelItem> modelItems, int index, TView view)
        {
            modelItems = modelItems.ToArray();
            InsertViewItems(view, modelItems.Select(x => CreateBindedViewItem(view, model, x)), model, modelItems, index);
        }

        protected void ReplaceItems(TModel model, IEnumerable<TModelItem> modelItems, int index, TView view)
        {
            modelItems = modelItems.ToArray();
            ReplaceViewItems(view, modelItems.Select(x => CreateBindedViewItem(view, model, x)), model, modelItems, index);
        }

        protected void MoveItems(TModel model, IEnumerable<TModelItem> modelItems, int index, TView view)
        {
            modelItems = modelItems.ToArray();
            MoveViewItems(view, modelItems.Select(x => GetBindedViewItem(view, model, x)), model, modelItems, index);
        }
    }
}
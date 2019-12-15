using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Simulacra.Binding.Base;

namespace Simulacra.Binding.Collection
{
    public class CollectionBindingModule<TModel, TView> : ObjectChangedBindingModuleBase<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        public CollectionBindingModule(TModel model, Dictionary<string, IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged>> bindings)
            : base(model, bindings)
        {
        }

        protected override void Subscribe(INotifyCollectionChanged eventSource) => eventSource.CollectionChanged += OnModelObjectChanged;
        protected override void Unsubscribe(INotifyCollectionChanged eventSource) => eventSource.CollectionChanged -= OnModelObjectChanged;
    }
}
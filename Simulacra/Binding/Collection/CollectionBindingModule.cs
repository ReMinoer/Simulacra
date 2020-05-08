using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Simulacra.Binding.Base;

namespace Simulacra.Binding.Collection
{
    public class CollectionBindingModule<TModel, TView> : SubscriptionBindingModuleBase<TModel, TView, IOneWaySubscriptionBinding<TModel, TView, INotifyCollectionChanged, NotifyCollectionChangedEventArgs>, INotifyCollectionChanged, NotifyCollectionChangedEventHandler>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        public CollectionBindingModule(TModel model, Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, INotifyCollectionChanged, NotifyCollectionChangedEventArgs>> bindings)
            : base(model, bindings)
        {
        }

        protected override void Subscribe(INotifyCollectionChanged notifier, NotifyCollectionChangedEventHandler handler)
            => notifier.CollectionChanged += handler;
        protected override void Unsubscribe(INotifyCollectionChanged notifier, NotifyCollectionChangedEventHandler handler)
            => notifier.CollectionChanged -= handler;

        protected override NotifyCollectionChangedEventHandler GetHandler(IOneWaySubscriptionBinding<TModel, TView, INotifyCollectionChanged, NotifyCollectionChangedEventArgs> binding)
            => (sender, e) => binding.UpdateView(Model, View, e);
    }
}
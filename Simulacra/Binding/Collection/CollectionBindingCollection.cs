using System.Collections.Generic;
using System.Collections.Specialized;

namespace Simulacra.Binding.Collection
{
    public class CollectionBindingCollection<TModel, TView> : Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, INotifyCollectionChanged, NotifyCollectionChangedEventArgs>>, ICollectionBindingsProvider<TModel, TView>
    {
        CollectionBindingCollection<TModel, TView> ICollectionBindingsProvider<TModel, TView>.CollectionBindings => this;
    }
}
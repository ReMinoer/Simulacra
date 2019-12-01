using System.Collections.Generic;
using System.Collections.Specialized;

namespace Simulacra.Binding.Collection
{
    public class CollectionBindingCollection<TModel, TView> : Dictionary<string, IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged>>
    {
    }
}
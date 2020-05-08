using System.Collections.Generic;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class ArrayBindingCollection<TModel, TView> : Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, INotifyArrayChanged, ArrayChangedEventArgs>>
    {
    }
}
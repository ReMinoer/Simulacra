using System.Collections.Generic;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class ArrayBindingCollection<TModel, TView> : Dictionary<string, IOneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged>>
    {
    }
}
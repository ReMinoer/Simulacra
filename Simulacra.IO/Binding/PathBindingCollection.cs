using System.Collections.Generic;
using Simulacra.Binding;

namespace Simulacra.IO.Binding
{
    public class PathBindingCollection<TModel, TView> : Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, string>>, IPathBindingsProvider<TModel, TView>
    {
        PathBindingCollection<TModel, TView> IPathBindingsProvider<TModel, TView>.PathBindings => this;
    }
}
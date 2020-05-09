using System.Collections.Generic;

namespace Simulacra.Binding.Property
{
    public class PropertyBindingCollection<TModel, TView> : Dictionary<string, IOneWayBinding<TModel, TView>>, IPropertyBindingsProvider<TModel, TView>
    {
        PropertyBindingCollection<TModel, TView> IPropertyBindingsProvider<TModel, TView>.PropertyBindings => this;
    }
}
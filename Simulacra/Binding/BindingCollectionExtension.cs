using System;
using System.Collections.Generic;

namespace Simulacra.Binding
{
    static public class BindingCollectionExtension
    {
        static public IOneWayBinding<TModel, TView> Add<TModel, TView>(
            this ICollection<IOneWayBinding<TModel, TView>> bindingCollection,
            Action<TModel, TView> viewAction)
        {
            var binding = new OneWayBinding<TModel, TView>(viewAction);
            bindingCollection.Add(binding);
            return binding;
        }
    }
}
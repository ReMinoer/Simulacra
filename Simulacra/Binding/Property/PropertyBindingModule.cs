using System.Collections.Generic;
using System.ComponentModel;
using Simulacra.Binding.Base;

namespace Simulacra.Binding.Property
{
    public class PropertyBindingModule<TModel, TView> : ObjectChangingBindingModuleBase<TModel, TView, IOneWayBinding<TModel, TView>>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        public PropertyBindingModule(TModel model, IReadOnlyDictionary<string, IOneWayBinding<TModel, TView>> bindings)
            : base(model, bindings) {}

        protected override void BindObject(IOneWayBinding<TModel, TView> binding) { }
        protected override void UnbindObject(IOneWayBinding<TModel, TView> binding) { }
        protected override void UnbindAllObjects() { }
    }
}
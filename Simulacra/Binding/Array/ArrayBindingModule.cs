using System.Collections.Generic;
using System.ComponentModel;
using Simulacra.Binding.Base;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class ArrayBindingModule<TModel, TView> : ObjectChangedBindingModuleBase<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        public ArrayBindingModule(TModel model, Dictionary<string, IOneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged>> bindings)
            : base(model, bindings)
        {
        }

        protected override void Subscribe(INotifyArrayChanged eventSource) => eventSource.ArrayChanged += OnModelObjectChanged;
        protected override void Unsubscribe(INotifyArrayChanged eventSource) => eventSource.ArrayChanged -= OnModelObjectChanged;
    }
}
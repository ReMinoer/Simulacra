using System.Collections.Generic;
using System.ComponentModel;
using Simulacra.Binding.Base;
using Simulacra.Utils;

namespace Simulacra.Binding.Array
{
    public class ArrayBindingModule<TModel, TView> : SubscriptionBindingModuleBase<TModel, TView, IOneWaySubscriptionBinding<TModel, TView, INotifyArrayChanged, ArrayChangedEventArgs>, INotifyArrayChanged, ArrayChangedEventHandler>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        public ArrayBindingModule(TModel model, Dictionary<string, IOneWaySubscriptionBinding<TModel, TView, INotifyArrayChanged, ArrayChangedEventArgs>> bindings)
            : base(model, bindings)
        {
        }

        protected override void Subscribe(INotifyArrayChanged notifier, ArrayChangedEventHandler handler)
            => notifier.ArrayChanged += handler;
        protected override void Unsubscribe(INotifyArrayChanged notifier, ArrayChangedEventHandler handler)
            => notifier.ArrayChanged -= handler;

        protected override ArrayChangedEventHandler GetHandler(IOneWaySubscriptionBinding<TModel, TView, INotifyArrayChanged, ArrayChangedEventArgs> binding)
            => (sender, e) => binding.UpdateView(Model, View, e);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Simulacra.Binding.Base
{
    public abstract class SubscriptionBindingModuleBase<TModel, TView, TBinding, TSubscription, THandler> : ObjectChangingBindingModuleBase<TModel, TView, TBinding>
        where TModel : class, INotifyPropertyChanged
        where TView : class
        where TBinding : class, IOneWaySubscriptionBinding<TModel, TView, TSubscription>
        where TSubscription : class
        where THandler : Delegate
    {
        protected readonly Dictionary<TBinding, TSubscription> SubscriptionBindings;
        protected readonly Dictionary<TBinding, THandler> BindingHandlers;

        protected SubscriptionBindingModuleBase(TModel model, IReadOnlyDictionary<string, TBinding> bindings)
            : base(model, bindings)
        {
            SubscriptionBindings = new Dictionary<TBinding, TSubscription>();
            BindingHandlers = new Dictionary<TBinding, THandler>();
        }

        protected abstract void Subscribe(TSubscription subscription, THandler handler);
        protected abstract void Unsubscribe(TSubscription subscription, THandler handler);
        protected abstract THandler GetHandler(TBinding binding);

        protected override void BindObject(TBinding binding)
        {
            TSubscription subscription = binding.GetSubscription(Model);
            if (subscription == null)
                return;

            THandler handler = GetHandler(binding);
            Subscribe(subscription, handler);

            SubscriptionBindings.Add(binding, subscription);
            BindingHandlers.Add(binding, handler);
        }

        protected override void UnbindObject(TBinding binding)
        {
            if (!BindingHandlers.TryGetValue(binding, out THandler handler))
                return;

            Unsubscribe(SubscriptionBindings[binding], handler);

            BindingHandlers.Remove(binding);
            SubscriptionBindings.Remove(binding);
        }

        protected override void UnbindAllObjects()
        {
            foreach (TBinding binding in SubscriptionBindings.Keys)
                Unsubscribe(SubscriptionBindings[binding], BindingHandlers[binding]);

            SubscriptionBindings.Clear();
            BindingHandlers.Clear();
        }
    }
}
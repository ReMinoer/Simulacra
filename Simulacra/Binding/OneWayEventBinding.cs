using System;

namespace Simulacra.Binding
{
    public class OneWaySubscriptionBinding<TModel, TView, TSubscription> : IOneWaySubscriptionBinding<TModel, TView, TSubscription>
    {
        private readonly IOneWayBinding<TModel, TView> _binding;
        private readonly Func<TModel, TSubscription> _subscriptionGetter;

        public OneWaySubscriptionBinding(IOneWayBinding<TModel, TView> binding, Func<TModel, TSubscription> subscriptionGetter)
        {
            _binding = binding;
            _subscriptionGetter = subscriptionGetter;
        }

        public TSubscription GetSubscription(TModel model) => _subscriptionGetter(model);
        public void SetView(TModel model, TView view) => _binding.SetView(model, view);
    }

    public class OneWaySubscriptionBinding<TModel, TView, TSubscription, TNotification> : OneWaySubscriptionBinding<TModel, TView, TSubscription>, IOneWaySubscriptionBinding<TModel, TView, TSubscription, TNotification>
    {
        private readonly IOneWayBinding<TModel, TView, TNotification> _notificationBinding;

        public OneWaySubscriptionBinding(IOneWayBinding<TModel, TView, TNotification> notificationBinding, Func<TModel, TSubscription> subscriptionGetter)
            : base(notificationBinding, subscriptionGetter)
        {
            _notificationBinding = notificationBinding;
        }

        public void UpdateView(TModel model, TView view, TNotification notification) => _notificationBinding.UpdateView(model, view, notification);
    }
}
using System;

namespace Simulacra.Binding
{
    static public class OneWayBindingExtension
    {
        static public OneWaySubscriptionBinding<TModel, TView, TSubscription> AsSubscriptionBinding<TModel, TView, TSubscription>(
            this IOneWayBinding<TModel, TView> binding,
            Func<TModel, TSubscription> subscriptionGetter)
        {
            return new OneWaySubscriptionBinding<TModel, TView, TSubscription>(binding, subscriptionGetter);
        }

        static public OneWaySubscriptionBinding<TModel, TView, TSubscription, TNotification> AsSubscriptionBinding<TModel, TView, TSubscription, TNotification>(
            this IOneWayBinding<TModel, TView, TNotification> binding,
            Func<TModel, TSubscription> subscriptionGetter)
        {
            return new OneWaySubscriptionBinding<TModel, TView, TSubscription, TNotification>(binding, subscriptionGetter);
        }
    }
}
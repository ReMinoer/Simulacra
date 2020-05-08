namespace Simulacra.Binding
{
    public interface ITwoWayBinding<in TModel, in TView> : IOneWayBinding<TModel, TView>
    {
        void InitializeModel(TModel model, TView view);
    }

    public interface ITwoWayBinding<in TModel, in TView, in TNotification> : ITwoWayBinding<TModel, TView>, IOneWayBinding<TModel, TView, TNotification>
    {
        void UpdateModel(TModel model, TView view, TNotification notification);
    }

    public interface ITwoWaySubscriptionBinding<in TModel, in TView, out TSubscription> : ITwoWayBinding<TModel, TView>, IOneWaySubscriptionBinding<TModel, TView, TSubscription>
    {
        TSubscription GetViewSubscription(TModel model);
    }

    public interface ITwoWaySubscriptionBinding<in TModel, in TView, out TSubscription, in TNotification> : ITwoWaySubscriptionBinding<TModel, TView, TSubscription>, ITwoWayBinding<TModel, TView, TNotification>, IOneWaySubscriptionBinding<TModel, TView, TSubscription, TNotification>
    {
    }
}
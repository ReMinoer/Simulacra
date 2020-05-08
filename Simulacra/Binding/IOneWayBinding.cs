namespace Simulacra.Binding
{
    public interface IOneWayBinding<in TModel, in TView>
    {
        void SetView(TModel model, TView view);
    }

    public interface IOneWayBinding<in TModel, in TView, in TNotification> : IOneWayBinding<TModel, TView>
    {
        void UpdateView(TModel model, TView view, TNotification notification);
    }

    public interface IOneWaySubscriptionBinding<in TModel, in TView, out TSubscription> : IOneWayBinding<TModel, TView>
    {
        TSubscription GetSubscription(TModel model);
    }

    public interface IOneWaySubscriptionBinding<in TModel, in TView, out TSubscription, in TNotification> : IOneWaySubscriptionBinding<TModel, TView, TSubscription>, IOneWayBinding<TModel, TView, TNotification>
    {
    }
}
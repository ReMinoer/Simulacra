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

    public interface IOneWayEventBinding<in TModel, in TView, in TNotification, out TEventSource> : IOneWayBinding<TModel, TView, TNotification>
    {
        TEventSource GetModelEventSource(TModel model);
    }
}
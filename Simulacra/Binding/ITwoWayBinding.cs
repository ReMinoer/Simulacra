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

    public interface ITwoWayEventBinding<in TModel, in TView, in TNotification, out TModelEventSource, out TViewEventSource> : ITwoWayBinding<TModel, TView, TNotification>, IOneWayEventBinding<TModel, TView, TNotification, TModelEventSource>
    {
        TViewEventSource GetViewEventSource(TModel model);
    }
}
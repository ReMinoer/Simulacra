using System;

namespace Simulacra.Binding
{
    public class OneWayEventBinding<TModel, TView, TNotification, TEventSource> : IOneWayEventBinding<TModel, TView, TNotification, TEventSource>
    {
        private readonly IOneWayBinding<TModel, TView, TNotification> _binding;
        private readonly Func<TModel, TEventSource> _eventSourceGetter;

        public OneWayEventBinding(IOneWayBinding<TModel, TView, TNotification> binding, Func<TModel, TEventSource> eventSourceGetter)
        {
            _binding = binding;
            _eventSourceGetter = eventSourceGetter;
        }

        public TEventSource GetModelEventSource(TModel model) => _eventSourceGetter(model);

        public void SetView(TModel model, TView view) => _binding.SetView(model, view);
        public void UpdateView(TModel model, TView view, TNotification notification) => _binding.UpdateView(model, view, notification);
    }
}
using System;

namespace Simulacra.Binding
{
    static public class OneWayBindingExtension
    {
        static public OneWayEventBinding<TModel, TView, TNotification, TEventSource> AsEventBinding<TModel, TView, TNotification, TEventSource>(
            this IOneWayBinding<TModel, TView, TNotification> binding,
            Func<TModel, TEventSource> eventSourceGetter)
        {
            return new OneWayEventBinding<TModel, TView, TNotification, TEventSource>(binding, eventSourceGetter);
        }
    }
}
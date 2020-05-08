using System;

namespace Simulacra.Binding
{
    public interface IBindingBuilder<out TModel, out TView>
    {
        void Do(Action<TModel, TView> action);
    }

    public interface IBindingBuilder<TModel, TView, TModelValue> : IBindingBuilder<TModel, TView>
    {
        Func<TModel, TView, TModelValue> Getter { get; set; }
    }

    public interface IBindingBuilder<TModel, TView, TModelValue, out TBindingCollection> : IBindingBuilder<TModel, TView, TModelValue>
    {
        IBindingBuilder<TModel, TView, TConvertedValue, TBindingCollection> Select<TConvertedValue>(Func<TModelValue, TModel, TView, TConvertedValue> converter);
    }
}
using System;

namespace Simulacra.Binding
{
    static public class BindingBuilderExtension
    {
        static public void Do<TModel, TView>(
            this IBindingBuilder<TModel, TView> builder,
            Action action)
        {
            builder.Do((m, v) => action());
        }

        static public void Do<TModel, TView>(
            this IBindingBuilder<TModel, TView> builder,
            Action<TView> action)
        {
            builder.Do((m, v) => action(v));
        }

        static public void Do<TModel, TView, TModelValue>(
            this IBindingBuilder<TModel, TView, TModelValue> builder,
            Action<TModelValue, TView> action)
        {
            Func<TModel, TView, TModelValue> getter = builder.Getter;
            builder.Do((m, v) => action(getter(m, v), v));
        }

        static public void Do<TModel, TView, TModelValue>(
            this IBindingBuilder<TModel, TView, TModelValue> builder,
            Action<TModelValue, TModel, TView> action)
        {
            Func<TModel, TView, TModelValue> getter = builder.Getter;
            builder.Do((m, v) => action(getter(m, v), m, v));
        }

        static public IBindingBuilder<TModel, TView, TConvertedValue, TBindingCollection> Select<TModel, TView, TModelValue, TConvertedValue, TBindingCollection>(
            this IBindingBuilder<TModel, TView, TModelValue, TBindingCollection> builder,
            Func<TModelValue, TConvertedValue> converter)
        {
            return builder.Select((mv, m, v) => converter(mv));
        }

        static public IBindingBuilder<TModel, TView, TCreated, TBindingCollection> Create<TModel, TView, TCreator, TCreated, TBindingCollection>(
            this IBindingBuilder<TModel, TView, TCreator, TBindingCollection> binding)
            where TCreator : ICreator<TCreated>
            where TCreated : class
        {
            return binding.Select(c => c?.Create());
        }
    }
}
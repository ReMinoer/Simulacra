using System;
using System.Linq.Expressions;
using Simulacra.Binding.Property;

namespace Simulacra.Binding
{
    static public class PropertyBindingBuilder
    {
        static public PropertyBindingBuilder<TModel, TView, TModelValue> From<TModel, TView, TModelValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelPropertyGetterExpression)
        {
            var memberExpression = (MemberExpression)modelPropertyGetterExpression.Body;
            string propertyName = memberExpression.Member.Name;

            Func<TModel, TModelValue> getter = modelPropertyGetterExpression.Compile();

            return new PropertyBindingBuilder<TModel, TView, TModelValue>(bindingCollection, propertyName, (m, v) => getter(m));
        }
    }

    static public class PropertyBindingBuilderExtension
    {
        static public void Do<TModel, TView>(
            this IPropertyBindingBuilder<TModel, TView> builder,
            Action action)
        {
            builder.Do((m, v) => action());
        }

        static public void Do<TModel, TView>(
            this IPropertyBindingBuilder<TModel, TView> builder,
            Action<TView> action)
        {
            builder.Do((m, v) => action(v));
        }

        static public void Do<TModel, TView, TModelValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> builder,
            Action<TModelValue, TView> action)
        {
            Func<TModel, TView, TModelValue> getter = builder.Getter;
            builder.Do((m, v) => action(getter(m, v), v));
        }

        static public void Do<TModel, TView, TModelValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> builder,
            Action<TModelValue, TModel, TView> action)
        {
            Func<TModel, TView, TModelValue> getter = builder.Getter;
            builder.Do((m, v) => action(getter(m, v), m, v));
        }

        static public PropertyBindingBuilder<TModel, TView, TConvertedValue> Select<TModel, TView, TModelValue, TConvertedValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> builder,
            Func<TModelValue, TConvertedValue> converter)
        {
            return builder.Select((mv, m, v) => converter(mv));
        }

        static public PropertyBindingBuilder<TModel, TView, TConvertedValue> Select<TModel, TView, TModelValue, TConvertedValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> builder,
            Func<TModelValue, TModel, TView, TConvertedValue> converter)
        {
            var newBuilder = new PropertyBindingBuilder<TModel, TView, TConvertedValue>(builder);

            Func<TModel, TView, TModelValue> getter = builder.Getter;
            IPropertyBindingBuilder<TModel, TView, TConvertedValue> interfaceBuilder = newBuilder;
            interfaceBuilder.Getter = (m, v) => converter(getter(m, v), m, v);

            return newBuilder;
        }

        static public PropertyBindingBuilder<TModel, TView, TCreated> Create<TModel, TView, TCreator, TCreated>(
            this IPropertyBindingBuilder<TModel, TView, TCreator> binding)
            where TCreator : ICreator<TCreated>
            where TCreated : class
        {
            return binding.Select(c => c?.Create());
        }
    }

    public interface IPropertyBindingBuilder<TModel, TView>
    {
        PropertyBindingCollection<TModel, TView> BindingCollection { get; }
        string PropertyName { get; set; }
        void Do(Action<TModel, TView> action);
    }

    public interface IPropertyBindingBuilder<TModel, TView, TModelValue> : IPropertyBindingBuilder<TModel, TView>
    {
        Func<TModel, TView, TModelValue> Getter { get; set; }
    }

    public class PropertyBindingBuilder<TModel, TView, TModelValue> : IPropertyBindingBuilder<TModel, TView, TModelValue>
    {
        private readonly PropertyBindingCollection<TModel, TView> _bindingCollection;
        private string _propertyName;
        private Func<TModel, TView, TModelValue> _getter;

        public PropertyBindingBuilder(PropertyBindingCollection<TModel, TView> bindingCollection, string propertyName, Func<TModel, TView, TModelValue> getter)
        {
            _bindingCollection = bindingCollection;
            _propertyName = propertyName;
            _getter = getter;
        }

        public PropertyBindingBuilder(IPropertyBindingBuilder<TModel, TView> builder)
        {
            _bindingCollection = builder.BindingCollection;
            _propertyName = builder.PropertyName;
        }

        public void Do(Action<TModel, TView> action)
        {
            _bindingCollection.Add(_propertyName, new OneWayBinding<TModel, TView>(action));
        }

        #region Explicit

        PropertyBindingCollection<TModel, TView> IPropertyBindingBuilder<TModel, TView>.BindingCollection => _bindingCollection;
        string IPropertyBindingBuilder<TModel, TView>.PropertyName
        {
            get => _propertyName;
            set => _propertyName = value;
        }
        Func<TModel, TView, TModelValue> IPropertyBindingBuilder<TModel, TView, TModelValue>.Getter
        {
            get => _getter;
            set => _getter = value;
        }

        #endregion
    }
}
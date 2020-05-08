using System;
using System.Linq.Expressions;
using Simulacra.Binding.Base;
using Simulacra.Binding.Property;
using Simulacra.Binding.Utils;

namespace Simulacra.Binding
{
    static public class PropertyBindingCollectionExtension
    {
        static public PropertyBindingBuilder<TModel, TView, TModelValue> From<TModel, TView, TModelValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelPropertyGetterExpression)
        {
            string propertyName = ExpressionUtils.GetPropertyMemberExpression(modelPropertyGetterExpression).Member.Name;
            Func<TModel, TModelValue> getter = modelPropertyGetterExpression.Compile();

            return new PropertyBindingBuilder<TModel, TView, TModelValue>(bindingCollection, propertyName, (m, v) => getter(m));
        }
    }

    public interface IPropertyBindingBuilder<TModel, TView, TModelValue> : IBindingBuilder<TModel, TView, TModelValue, PropertyBindingCollection<TModel, TView>>
    {
    }

    public class PropertyBindingBuilder<TModel, TView, TModelValue> : BindingBuilderBase<TModel, TView, TModelValue, PropertyBindingCollection<TModel, TView>>, IPropertyBindingBuilder<TModel, TView, TModelValue>
    {
        public PropertyBindingBuilder(PropertyBindingCollection<TModel, TView> bindingCollection, string propertyName, Func<TModel, TView, TModelValue> getter)
            : base(bindingCollection, propertyName, getter) {}

        public override void Do(Action<TModel, TView> action)
        {
            _bindingCollection.Add(_propertyName, new OneWayBinding<TModel, TView>(action));
        }

        public override IBindingBuilder<TModel, TView, TConvertedValue, PropertyBindingCollection<TModel, TView>> Select<TConvertedValue>(
            Func<TModelValue, TModel, TView, TConvertedValue> converter)
        {
            return new PropertyBindingBuilder<TModel, TView, TConvertedValue>(_bindingCollection, _propertyName, (m, v) => converter(_getter(m, v), m, v));
        }
    }
}
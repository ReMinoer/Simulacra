using System;
using System.Linq.Expressions;
using Simulacra.Binding.Property;

namespace Simulacra.Binding
{
    static public class PropertyBindingCollectionExtension
    {
        static public IOneWayBinding<TModel, TView> AddProperty<TModel, TView, TModelValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelGetterExpression,
            Action<TModel, TView, TModelValue> viewAction)
        {
            string modelPropertyName = GetMemberExpression(modelGetterExpression).Member.Name;
            Func<TModel, TModelValue> getter = modelGetterExpression.Compile();
            
            var binding = new OneWayBinding<TModel, TView>((m, v) => viewAction(m, v, getter(m)));
            bindingCollection.Add(modelPropertyName, binding);
            return binding;
        }

        static public IOneWayBinding<TModel, TView> AddProperty<TModel, TView, TModelValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelGetterExpression,
            Action<TView> viewAction)
        {
            string modelPropertyName = GetMemberExpression(modelGetterExpression).Member.Name;
            
            var binding = new OneWayBinding<TModel, TView>((m, v) => viewAction(v));
            bindingCollection.Add(modelPropertyName, binding);
            return binding;
        }

        static public MemberExpression GetMemberExpression<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            return (MemberExpression)expression.Body;
        }
    }
}
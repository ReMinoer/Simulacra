using System;
using System.Linq.Expressions;
using System.Reflection;
using Niddle;
using Niddle.Injectables;
using Niddle.Injectables.Expressions;
using Simulacra.Binding;
using Simulacra.Binding.Property;

namespace Simulacra.Injection.Binding
{
    static public class PropertyBindingCollectionExtension
    {
        static public IOneWayBinding<TModel, TView> AddProperty<TModel, TView, TModelValue, TViewValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelGetterExpression,
            Expression<Func<TView, TViewValue>> viewGetterExpression,
            Func<TModelValue, TModel, TView, TViewValue> converter,
            IInjectionExpression injectionExpression = null)
        {
            string modelPropertyName = GetMemberExpression(modelGetterExpression).Member.Name;
            Func<TModel, TModelValue> getter = modelGetterExpression.Compile();

            var memberExpression = (MemberExpression)viewGetterExpression.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            Delegate targetSelector = Expression.Lambda(memberExpression.Expression, viewGetterExpression.Parameters).Compile();

            InjectableProperty<TView, TViewValue> injectable = propertyInfo.AsInjectable<TView, TViewValue>(targetSelector, injectionExpression);
            
            return bindingCollection.AddBase(modelPropertyName, getter, converter, injectable);
        }

        static public IOneWayBinding<TModel, TView> AddProperty<TModel, TView, TValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TValue>> modelGetterExpression,
            Expression<Func<TView, TValue>> viewGetterExpression,
            IInjectionExpression injectionExpression = null)
        {
            return bindingCollection.AddProperty(modelGetterExpression, viewGetterExpression, (mv, m, v) => mv, injectionExpression);
        }
        
        static public IOneWayBinding<TModel, TView> AddPropertyCreator<TModel, TView, TModelValue, TViewValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelGetterExpression,
            Expression<Func<TView, TViewValue>> viewGetterExpression)
            where TModelValue : ICreator<TViewValue>
        {
            return bindingCollection.AddProperty(modelGetterExpression, viewGetterExpression, (mv, m, v) => mv.Create());
        }
        
        static public IOneWayBinding<TModel, TView> AddPropertyConfigurator<TModel, TView, TModelValue, TViewValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, TModelValue>> modelGetterExpression,
            Expression<Func<TView, TViewValue>> viewGetterExpression)
            where TModelValue : IConfigurator<TViewValue>
        {
            string modelPropertyName = GetMemberExpression(modelGetterExpression).Member.Name;
            Func<TModel, TModelValue> getter = modelGetterExpression.Compile();
            InjectableProperty<TView, TModelValue> injectable = GetPropertyInfo(viewGetterExpression).AsInjectable<TView, TModelValue>(new ConfiguratorInjection());

            return bindingCollection.AddBase(modelPropertyName, getter, (mv, m, v) => mv, injectable);
        }

        private class ConfiguratorInjection : IInjectionExpression
        {
            static private readonly MethodInfo ConfigureMethodInfo;

            static ConfiguratorInjection()
            {
                ConfigureMethodInfo = typeof(IConfigurator<>).GetTypeInfo().GetDeclaredMethod(nameof(IConfigurator<object>.Configure));
            }

            public Expression BuildInjectionExpression(Expression targetExpression, Expression valueExpression, Type memberType)
            {
                return Expression.Call(valueExpression, ConfigureMethodInfo, targetExpression);
            }
        }

        static private IOneWayBinding<TModel, TView> AddBase<TModel, TView, TModelValue, TViewValue>(
            this PropertyBindingCollection<TModel, TView> bindingCollection,
            string modelPropertyName,
            Func<TModel, TModelValue> getter,
            Func<TModelValue, TModel, TView, TViewValue> converter,
            IInjectable<TView, TViewValue> injectable)
        {
            var binding = new OneWayBinding<TModel, TView>((m, v) => injectable.Inject(v, converter(getter(m), m, v)));
            bindingCollection.Add(modelPropertyName, binding);
            return binding;
        }

        static public MemberExpression GetMemberExpression<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            return (MemberExpression)expression.Body;
        }

        static public PropertyInfo GetPropertyInfo<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            return (PropertyInfo)GetMemberExpression(expression).Member;
        }
    }
}
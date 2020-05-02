using System;
using System.Linq.Expressions;
using System.Reflection;
using Niddle;
using Niddle.Injectables;
using Niddle.Injectables.Expressions;
using Simulacra.Binding;

namespace Simulacra.Injection.Binding
{
    static public class PropertyBindingsExtension
    {
        static public void To<TModel, TView, TModelValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> binding,
            Expression<Func<TView, TModelValue>> viewGetterExpression,
            IInjectionExpression injectionExpression = null)
        {
            var memberExpression = (MemberExpression)viewGetterExpression.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            Delegate targetSelector = Expression.Lambda(memberExpression.Expression, viewGetterExpression.Parameters).Compile();

            InjectableProperty<TView, TModelValue> injectable = propertyInfo.AsInjectable<TView, TModelValue>(targetSelector, injectionExpression);

            Func<TModel, TView, TModelValue> getter = binding.Getter;
            binding.Do((m, v) => injectable.Inject(v, getter(m, v)));
        }

        static public void Configure<TModel, TView, TModelValue, TViewValue>(
            this IPropertyBindingBuilder<TModel, TView, TModelValue> binding,
            Expression<Func<TView, TModelValue>> viewGetterExpression)
            where TModelValue : IConfigurator<TViewValue>
        {
            binding.To(viewGetterExpression, new ConfiguratorInjection());
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
    }
}
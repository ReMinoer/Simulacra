using System;
using System.Linq.Expressions;
using Simulacra.Binding;
using Simulacra.Binding.Base;
using Simulacra.Binding.Utils;

namespace Simulacra.IO.Binding
{
    static public class PathBindingCollectionExtension
    {
        static public PathBindingBuilder<TModel, TView, string> From<TModel, TView>(
            this IPathBindingsProvider<TModel, TView> pathBindingsProvider,
            Expression<Func<TModel, string>> modelPropertyGetterExpression)
        {
            return pathBindingsProvider.From(modelPropertyGetterExpression, x => x);
        }

        static public PathBindingBuilder<TModel, TView, TModelValue> From<TModel, TView, TModelValue>(
            this IPathBindingsProvider<TModel, TView> pathBindingsProvider,
            Expression<Func<TModel, TModelValue>> modelPropertyGetterExpression,
            Expression<Func<TModelValue, string>> modelPropertyPathGetter)
        {
            MemberExpression memberExpression = ExpressionUtils.GetPropertyMemberExpression(modelPropertyGetterExpression);
            string propertyName = memberExpression.Member.Name;

            InvocationExpression pathGetterBody = Expression.Invoke(modelPropertyPathGetter, modelPropertyGetterExpression.Body);
            Func<TModel, string> pathGetter = Expression.Lambda<Func<TModel, string>>(pathGetterBody, modelPropertyGetterExpression.Parameters).Compile();
            Func<TModel, TModelValue> getter = modelPropertyGetterExpression.Compile();

            return new PathBindingBuilder<TModel, TView, TModelValue>(pathBindingsProvider.PathBindings, propertyName, (m, v) => getter(m), pathGetter);
        }
    }

    public interface IPathBindingBuilder<TModel, TView, TModelValue> : IBindingBuilder<TModel, TView, TModelValue, PathBindingCollection<TModel, TView>>
    {
        Func<TModel, string> PathGetter { get; set; }
    }

    public class PathBindingBuilder<TModel, TView, TModelValue> : BindingBuilderBase<TModel, TView, TModelValue, PathBindingCollection<TModel, TView>>, IPathBindingBuilder<TModel, TView, TModelValue>
    {
        private Func<TModel, string> _pathGetter;

        public PathBindingBuilder(PathBindingCollection<TModel, TView> bindingCollection, string propertyName, Func<TModel, TView, TModelValue> getter, Func<TModel, string> pathGetter)
            : base(bindingCollection, propertyName, getter)
        {
            _pathGetter = pathGetter;
        }

        public override void Do(Action<TModel, TView> action)
        {
            _bindingCollection.Add(_propertyName, new OneWayBinding<TModel, TView>(action).AsSubscriptionBinding(_pathGetter));
        }

        public override IBindingBuilder<TModel, TView, TConvertedValue, PathBindingCollection<TModel, TView>> Select<TConvertedValue>(
            Func<TModelValue, TModel, TView, TConvertedValue> converter)
        {
            return new PathBindingBuilder<TModel, TView, TConvertedValue>(_bindingCollection, _propertyName, (m, v) => converter(_getter(m, v), m, v), _pathGetter);
        }

        #region Explicit

        Func<TModel, string> IPathBindingBuilder<TModel, TView, TModelValue>.PathGetter
        {
            get => _pathGetter;
            set => _pathGetter = value;
        }

        #endregion
    }
}
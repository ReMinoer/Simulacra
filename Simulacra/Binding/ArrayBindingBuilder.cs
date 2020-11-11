using System;
using System.Linq.Expressions;
using Simulacra.Binding.Array;
using Simulacra.Binding.Utils;
using Simulacra.Utils;

namespace Simulacra.Binding
{
    static public class ArrayBindingCollectionExtension
    {
        static public ArrayBindingBuilder<TModel, TView, TModelItem, TModelItem> FromArray<TModel, TView, TModelItem>(
            this IArrayBindingsProvider<TModel, TView> arrayBindingsProvider,
            Expression<Func<TModel, IArray<TModelItem>>> modelPropertyGetterExpression)
        {
            MemberExpression memberExpression = ExpressionUtils.GetPropertyMemberExpression(modelPropertyGetterExpression);
            string propertyName = memberExpression.Member.Name;

            UnaryExpression eventSourceGetterExpression = Expression.Convert(memberExpression, typeof(INotifyArrayChanged));
            Func<TModel, INotifyArrayChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyArrayChanged>>(eventSourceGetterExpression, modelPropertyGetterExpression.Parameters).Compile();
            Func<TModel, IArray<TModelItem>> referenceGetter = modelPropertyGetterExpression.Compile();
            
            return new ArrayBindingBuilder<TModel, TView, TModelItem, TModelItem>(arrayBindingsProvider.ArrayBindings, propertyName, eventSourceGetter, referenceGetter, (m, x, v, y) => x);
        }
    }

    static public class ArrayBindingBuilderExtension
    {
        static public void To<TModel, TView, TModelItem, TViewItem, TViewArray, TCreatedArray>(
            this IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> binding,
            Expression<Func<TView, TViewArray>> arrayGetterExpression,
            Action<TModel, TView, TViewArray, int[]> arrayResizer = null,
            Func<TModel, TView, int[], TCreatedArray> arrayCreator = null)
            where TViewArray : IWriteableArray<TViewItem>
            where TCreatedArray : TViewArray
        {
            Func<TView, TViewArray> arrayGetter = arrayGetterExpression.Compile();

            var arrayMemberExpression = (MemberExpression)arrayGetterExpression.Body;
            ParameterExpression setterValueParameter = Expression.Parameter(typeof(TViewArray));
            Expression setterBodyExpression = Expression.Assign(arrayMemberExpression, setterValueParameter);
            Action<TView, TViewArray> arraySetter = Expression.Lambda<Action<TView, TViewArray>>(setterBodyExpression, arrayGetterExpression.Parameters[0], setterValueParameter).Compile();

            binding.To(arrayGetter, arraySetter, arrayResizer, arrayCreator);
        }

        static public ArrayBindingBuilder<TModel, TView, TModelItem, TNewViewItem> SelectItems<TModel, TView, TModelItem, TOldViewItem, TNewViewItem>(
            this IArrayBindingBuilder<TModel, TView, TModelItem, TOldViewItem> builder,
            Func<TModel, TModelItem, TView, TNewViewItem, TNewViewItem> itemConverter,
            Action<TNewViewItem> viewItemDisposer = null)
        {
            var newBuilder = new ArrayBindingBuilder<TModel, TView, TModelItem, TNewViewItem>(builder);

            IArrayBindingBuilder<TModel, TView, TModelItem, TNewViewItem> interfaceBuilder = newBuilder;
            interfaceBuilder.ReferenceGetter = builder.ReferenceGetter;
            interfaceBuilder.ItemConverter = itemConverter;
            interfaceBuilder.ViewItemDisposer = viewItemDisposer;

            return newBuilder;
        }
    }

    public interface IArrayBindingBuilder<TModel, TView>
    {
        ArrayBindingCollection<TModel, TView> BindingCollection { get; }
        string ReferencePropertyName { get; set; }
        Func<TModel, INotifyArrayChanged> SubscriptionGetter { get; set; }
    }

    public interface IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> : IArrayBindingBuilder<TModel, TView>
    {
        Func<TModel, IArray<TModelItem>> ReferenceGetter { get; set; }
        Func<TModel, TModelItem, TView, TViewItem, TViewItem> ItemConverter { get; set; }
        Action<TViewItem> ViewItemDisposer { get; set; }

        void To<TViewArray, TCreatedArray>(
            Func<TView, TViewArray> arrayGetter,
            Action<TView, TViewArray> arraySetter,
            Action<TModel, TView, TViewArray, int[]> arrayResizer,
            Func<TModel, TView, int[], TCreatedArray> arrayCreator)
            where TViewArray : IWriteableArray<TViewItem>
            where TCreatedArray : TViewArray;
    }

    public class ArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> : IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>
    {
        private readonly ArrayBindingCollection<TModel, TView> _bindingCollection;
        private string _referencePropertyName;
        private Func<TModel, INotifyArrayChanged> _subscriptionGetter;

        private Func<TModel, IArray<TModelItem>> _referenceGetter;
        private Func<TModel, TModelItem, TView, TViewItem, TViewItem> _itemConverter;
        private Action<TViewItem> _viewItemDisposer;

        public ArrayBindingBuilder(
            ArrayBindingCollection<TModel, TView> bindingCollection,
            string referencePropertyName,
            Func<TModel, INotifyArrayChanged> subscriptionGetter,
            Func<TModel, IArray<TModelItem>> referenceGetter,
            Func<TModel, TModelItem, TView, TViewItem, TViewItem> itemConverter
        )
        {
            _bindingCollection = bindingCollection;
            _referencePropertyName = referencePropertyName;
            _referenceGetter = referenceGetter;
            _subscriptionGetter = subscriptionGetter;
            _itemConverter = itemConverter;
        }

        public ArrayBindingBuilder(IArrayBindingBuilder<TModel, TView> builder)
        {
            _bindingCollection = builder.BindingCollection;
            _referencePropertyName = builder.ReferencePropertyName;
            _subscriptionGetter = builder.SubscriptionGetter;
        }

        public void To<TViewArray, TCreatedArray>(
            Func<TView, TViewArray> arrayGetter,
            Action<TView, TViewArray> arraySetter,
            Action<TModel, TView, TViewArray, int[]> arrayResizer,
            Func<TModel, TView, int[], TCreatedArray> arrayCreator)
            where TViewArray : IWriteableArray<TViewItem>
            where TCreatedArray : TViewArray
        {
            _bindingCollection.Add(_referencePropertyName,
                new OneWayArrayBinding<TModel, TView, TModelItem, TViewItem, TViewArray, TCreatedArray>(
                    _referenceGetter,
                    arrayGetter,
                    arraySetter,
                    arrayResizer,
                    arrayCreator,
                    _itemConverter,
                    _viewItemDisposer
                    ).AsSubscriptionBinding(_subscriptionGetter));
        }

        #region Explicit

        ArrayBindingCollection<TModel, TView> IArrayBindingBuilder<TModel, TView>.BindingCollection => _bindingCollection;
        string IArrayBindingBuilder<TModel, TView>.ReferencePropertyName
        {
            get => _referencePropertyName;
            set => _referencePropertyName = value;
        }
        Func<TModel, INotifyArrayChanged> IArrayBindingBuilder<TModel, TView>.SubscriptionGetter
        {
            get => _subscriptionGetter;
            set => _subscriptionGetter = value;
        }
        Func<TModel, IArray<TModelItem>> IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>.ReferenceGetter
        {
            get => _referenceGetter;
            set => _referenceGetter = value;
        }
        Func<TModel, TModelItem, TView, TViewItem, TViewItem> IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>.ItemConverter
        {
            get => _itemConverter;
            set => _itemConverter = value;
        }
        Action<TViewItem> IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>.ViewItemDisposer
        {
            get => _viewItemDisposer;
            set => _viewItemDisposer = value;
        }

        #endregion
    }
}
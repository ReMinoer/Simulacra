using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using Simulacra.Binding.Collection;
using Simulacra.Binding.Utils;

namespace Simulacra.Binding
{
    static public class CollectionBindingCollectionExtension
    {
        static public CollectionBindingBuilder<TModel, TView, TModelItem, TModelItem> From<TModel, TView, TModelItem>(
            this CollectionBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IEnumerable<TModelItem>>> modelPropertyGetterExpression)
        {
            MemberExpression memberExpression = ExpressionUtils.GetPropertyMemberExpression(modelPropertyGetterExpression);
            string propertyName = memberExpression.Member.Name;

            UnaryExpression eventSourceGetterExpression = Expression.Convert(memberExpression, typeof(INotifyCollectionChanged));
            Func<TModel, INotifyCollectionChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyCollectionChanged>>(eventSourceGetterExpression, modelPropertyGetterExpression.Parameters).Compile();
            Func<TModel, IEnumerable<TModelItem>> referenceGetter = modelPropertyGetterExpression.Compile();

            return new CollectionBindingBuilder<TModel, TView, TModelItem, TModelItem>(bindingCollection, propertyName, eventSourceGetter, referenceGetter, (m, x, v) => x, EqualityComparer<TModelItem>.Default.Equals);
        }
    }

    static public class CollectionBindingBuilderExtension
    {
        static public CollectionBindingBuilder<TModel, TView, TModelItem, TNewViewItem> SelectItems<TModel, TView, TModelItem, TOldViewItem, TNewViewItem>(
            this ICollectionBindingBuilder<TModel, TView, TModelItem, TOldViewItem> builder,
            Func<TModel, TModelItem, TView, TNewViewItem> itemConverter,
            Func<TModelItem, TNewViewItem, bool> itemEquality,
            Action<TNewViewItem> viewItemDisposer = null)
        {
            var newBuilder = new CollectionBindingBuilder<TModel, TView, TModelItem, TNewViewItem>(builder);

            ICollectionBindingBuilder<TModel, TView, TModelItem, TNewViewItem> interfaceBuilder = newBuilder;
            interfaceBuilder.ReferenceGetter = builder.ReferenceGetter;
            interfaceBuilder.ItemConverter = itemConverter;
            interfaceBuilder.ItemEquality = itemEquality;
            interfaceBuilder.ViewItemDisposer = viewItemDisposer;

            return newBuilder;
        }
    }

    public interface ICollectionBindingBuilder<TModel, TView>
    {
        CollectionBindingCollection<TModel, TView> BindingCollection { get; }
        string ReferencePropertyName { get; set; }
        Func<TModel, INotifyCollectionChanged> SubscriptionGetter { get; set; }
    }

    public interface ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem> : ICollectionBindingBuilder<TModel, TView>
    {
        Func<TModel, IEnumerable<TModelItem>> ReferenceGetter { get; set; }
        Func<TModel, TModelItem, TView, TViewItem> ItemConverter { get; set; }
        Func<TModelItem, TViewItem, bool> ItemEquality { get; set; }
        Action<TViewItem> ViewItemDisposer { get; set; }

        void To(Func<TView, ICollection<TViewItem>> collectionGetter);
    }

    public class CollectionBindingBuilder<TModel, TView, TModelItem, TViewItem> : ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem>
    {
        private readonly CollectionBindingCollection<TModel, TView> _bindingCollection;
        private string _referencePropertyName;
        private Func<TModel, INotifyCollectionChanged> _subscriptionGetter;

        private Func<TModel, IEnumerable<TModelItem>> _referenceGetter;
        private Func<TModel, TModelItem, TView, TViewItem> _itemConverter;
        private Func<TModelItem, TViewItem, bool> _itemEquality;
        private Action<TViewItem> _viewItemDisposer;

        public CollectionBindingBuilder(
            CollectionBindingCollection<TModel, TView> bindingCollection,
            string referencePropertyName,
            Func<TModel, INotifyCollectionChanged> subscriptionGetter,
            Func<TModel, IEnumerable<TModelItem>> referenceGetter,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter,
            Func<TModelItem, TViewItem, bool> itemEquality
        )
        {
            _bindingCollection = bindingCollection;
            _referencePropertyName = referencePropertyName;
            _referenceGetter = referenceGetter;
            _subscriptionGetter = subscriptionGetter;
            _itemConverter = itemConverter;
            _itemEquality = itemEquality;
        }

        public CollectionBindingBuilder(ICollectionBindingBuilder<TModel, TView> builder)
        {
            _bindingCollection = builder.BindingCollection;
            _referencePropertyName = builder.ReferencePropertyName;
            _subscriptionGetter = builder.SubscriptionGetter;
        }

        public void To(Func<TView, ICollection<TViewItem>> collectionGetter)
        {
            _bindingCollection.Add(_referencePropertyName,
                new OneWayCollectionBinding<TModel, TView, TModelItem, TViewItem>(
                    _referenceGetter,
                    collectionGetter,
                    _itemConverter,
                    _itemEquality,
                    _viewItemDisposer
                    ).AsSubscriptionBinding(_subscriptionGetter));
        }

        #region Explicit

        CollectionBindingCollection<TModel, TView> ICollectionBindingBuilder<TModel, TView>.BindingCollection => _bindingCollection;
        string ICollectionBindingBuilder<TModel, TView>.ReferencePropertyName
        {
            get => _referencePropertyName;
            set => _referencePropertyName = value;
        }
        Func<TModel, INotifyCollectionChanged> ICollectionBindingBuilder<TModel, TView>.SubscriptionGetter
        {
            get => _subscriptionGetter;
            set => _subscriptionGetter = value;
        }
        Func<TModel, IEnumerable<TModelItem>> ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem>.ReferenceGetter
        {
            get => _referenceGetter;
            set => _referenceGetter = value;
        }
        Func<TModel, TModelItem, TView, TViewItem> ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem>.ItemConverter
        {
            get => _itemConverter;
            set => _itemConverter = value;
        }
        Func<TModelItem, TViewItem, bool> ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem>.ItemEquality
        {
            get => _itemEquality;
            set => _itemEquality = value;
        }
        Action<TViewItem> ICollectionBindingBuilder<TModel, TView, TModelItem, TViewItem>.ViewItemDisposer
        {
            get => _viewItemDisposer;
            set => _viewItemDisposer = value;
        }

        #endregion
    }
}
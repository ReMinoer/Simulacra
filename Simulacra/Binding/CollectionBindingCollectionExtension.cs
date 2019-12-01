using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using Simulacra.Binding.Collection;

namespace Simulacra.Binding
{
    static public class CollectionBindingCollectionExtension
    {
        static public IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> AddCollection<TModel, TView, TModelItem, TViewItem>(
            this CollectionBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IEnumerable<TModelItem>>> referenceGetterExpression,
            Expression<Func<TView, ICollection<TViewItem>>> collectionGetterExpression,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter,
            Func<TModelItem, TViewItem, bool> itemEquality,
            Action<TViewItem> viewItemDisposer = null)
        {
            MemberExpression memberExpression = GetMemberExpression(referenceGetterExpression);

            string modelPropertyName = memberExpression.Member.Name;
            Func<TModel, INotifyCollectionChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyCollectionChanged>>(memberExpression).Compile();
            Func<TModel, IEnumerable<TModelItem>> referenceGetter = referenceGetterExpression.Compile();
            Func<TView, ICollection<TViewItem>> collectionGetter = collectionGetterExpression.Compile();
            
            return bindingCollection.AddBase(modelPropertyName, eventSourceGetter, referenceGetter, collectionGetter, itemConverter, itemEquality, viewItemDisposer);
        }

        static public IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> AddCollection<TModel, TView, TItem>(
            this CollectionBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IEnumerable<TItem>>> referenceGetterExpression,
            Expression<Func<TView, ICollection<TItem>>> collectionGetterExpression,
            Func<TItem, TItem, bool> itemEquality = null,
            Action<TItem> viewItemDisposer = null)
        {
            MemberExpression memberExpression = GetMemberExpression(referenceGetterExpression);

            string modelPropertyName = memberExpression.Member.Name;
            Func<TModel, INotifyCollectionChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyCollectionChanged>>(memberExpression).Compile();
            Func<TModel, IEnumerable<TItem>> referenceGetter = referenceGetterExpression.Compile();
            Func<TView, ICollection<TItem>> collectionGetter = collectionGetterExpression.Compile();
            itemEquality = itemEquality ?? EqualityComparer<TItem>.Default.Equals;
            
            return bindingCollection.AddBase(modelPropertyName, eventSourceGetter, referenceGetter, collectionGetter, (m, x, v) => x, itemEquality, viewItemDisposer);
        }

        static private IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> AddBase<TModel, TView, TModelItem, TViewItem>(
            this CollectionBindingCollection<TModel, TView> bindingCollection,
            string modelPropertyName,
            Func<TModel, INotifyCollectionChanged> eventSourceGetter,
            Func<TModel, IEnumerable<TModelItem>> referenceGetter,
            Func<TView, ICollection<TViewItem>> collectionGetter,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter,
            Func<TModelItem, TViewItem, bool> itemEquality,
            Action<TViewItem> viewItemDisposer)
        {
            OneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> binding
                = new OneWayCollectionBinding<TModel, TView, TModelItem, TViewItem>(referenceGetter, collectionGetter, itemConverter, itemEquality, viewItemDisposer)
                    .AsEventBinding(eventSourceGetter);

            bindingCollection.Add(modelPropertyName, binding);
            return binding;
        }

        static public MemberExpression GetMemberExpression<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            return (MemberExpression)expression.Body;
        }
    }
}
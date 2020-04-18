using System;
using System.Linq.Expressions;
using Simulacra.Binding.Array;
using Simulacra.Utils;

namespace Simulacra.Binding
{
    static public class ArrayBindingCollectionExtension
    {
        static public IOneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged> AddArray<TModel, TView, TModelItem, TViewItem, TViewArray>(
            this ArrayBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IArray<TModelItem>>> referenceGetterExpression,
            Expression<Func<TView, IWriteableArray<TViewItem>>> arrayGetterExpression,
            Func<int[], TViewArray> arrayCreator,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter,
            Action<TViewItem> viewItemDisposer = null)
            where TViewArray : IWriteableArray<TViewItem>
        {
            MemberExpression referenceMemberExpression = GetMemberExpression(referenceGetterExpression);
            MemberExpression arrayMemberExpression = GetMemberExpression(arrayGetterExpression);

            string modelPropertyName = referenceMemberExpression.Member.Name;
            Func<TModel, INotifyArrayChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyArrayChanged>>(referenceMemberExpression, referenceGetterExpression.Parameters).Compile();
            Func<TModel, IArray<TModelItem>> referenceGetter = referenceGetterExpression.Compile();
            Func<TView, IWriteableArray<TViewItem>> collectionGetter = arrayGetterExpression.Compile();

            ParameterExpression setterValueParameter = Expression.Parameter(typeof(TViewArray));
            Expression setterBodyExpression = Expression.Assign(arrayMemberExpression, setterValueParameter);
            Action<TView, TViewArray> arraySetter = Expression.Lambda<Action<TView, TViewArray>>(setterBodyExpression, arrayGetterExpression.Parameters[0], setterValueParameter).Compile();
            
            return bindingCollection.AddBase(modelPropertyName, eventSourceGetter, referenceGetter, collectionGetter, arraySetter, arrayCreator, itemConverter, viewItemDisposer);
        }

        static public IOneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged> AddArray<TModel, TView, TItem, TViewArray>(
            this ArrayBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IArray<TItem>>> referenceGetterExpression,
            Expression<Func<TView, IWriteableArray<TItem>>> arrayGetterExpression,
            Func<int[], TViewArray> arrayCreator,
            Action<TItem> viewItemDisposer = null)
            where TViewArray : IWriteableArray<TItem>
        {
            return bindingCollection.AddArray(referenceGetterExpression, arrayGetterExpression, arrayCreator, (m, x, v) => x, viewItemDisposer);
        }

        static private IOneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged> AddBase<TModel, TView, TModelItem, TViewItem, TViewArray>(
            this ArrayBindingCollection<TModel, TView> bindingCollection,
            string modelPropertyName,
            Func<TModel, INotifyArrayChanged> eventSourceGetter,
            Func<TModel, IArray<TModelItem>> referenceGetter,
            Func<TView, IWriteableArray<TViewItem>> arrayGetter,
            Action<TView, TViewArray> arraySetter,
            Func<int[], TViewArray> arrayCreator,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter,
            Action<TViewItem> viewItemDisposer)
            where TViewArray : IWriteableArray<TViewItem>
        {
            OneWayEventBinding<TModel, TView, ArrayChangedEventArgs, INotifyArrayChanged> binding
                = new OneWayArrayBinding<TModel, TView, TModelItem, TViewItem, TViewArray>(referenceGetter, arrayGetter, arraySetter, arrayCreator, itemConverter, viewItemDisposer)
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
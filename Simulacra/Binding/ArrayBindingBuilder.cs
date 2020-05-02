using System;
using System.Linq.Expressions;
using Simulacra.Binding.Array;
using Simulacra.Utils;

namespace Simulacra.Binding
{
    static public class ArrayBindingBuilder
    {
        static public ArrayBindingBuilder<TModel, TView, TModelItem, TModelItem> From<TModel, TView, TModelItem>(
            this ArrayBindingCollection<TModel, TView> bindingCollection,
            Expression<Func<TModel, IArray<TModelItem>>> modelPropertyGetterExpression)
        {
            var memberExpression = (MemberExpression)modelPropertyGetterExpression.Body;
            string propertyName = memberExpression.Member.Name;

            UnaryExpression eventSourceGetterExpression = Expression.Convert(memberExpression, typeof(INotifyArrayChanged));
            Func<TModel, INotifyArrayChanged> eventSourceGetter = Expression.Lambda<Func<TModel, INotifyArrayChanged>>(eventSourceGetterExpression, modelPropertyGetterExpression.Parameters).Compile();
            Func<TModel, IArray<TModelItem>> referenceGetter = modelPropertyGetterExpression.Compile();
            
            return new ArrayBindingBuilder<TModel, TView, TModelItem, TModelItem>(bindingCollection, propertyName, eventSourceGetter, referenceGetter, (m, x, v) => x);
        }
    }

    static public class ArrayBindingBuilderExtension
    {
        static public void To<TModel, TView, TModelItem, TViewItem, TViewArray>(
            this IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> binding,
            Expression<Func<TView, IWriteableArray<TViewItem>>> arrayGetterExpression,
            Func<int[], TViewArray> arrayCreator)
            where TViewArray : IWriteableArray<TViewItem>
        {
            Func<TView, IWriteableArray<TViewItem>> arrayGetter = arrayGetterExpression.Compile();

            var arrayMemberExpression = (MemberExpression)arrayGetterExpression.Body;
            ParameterExpression setterValueParameter = Expression.Parameter(typeof(TViewArray));
            Expression setterBodyExpression = Expression.Assign(arrayMemberExpression, setterValueParameter);
            Action<TView, TViewArray> arraySetter = Expression.Lambda<Action<TView, TViewArray>>(setterBodyExpression, arrayGetterExpression.Parameters[0], setterValueParameter).Compile();

            binding.To(arrayGetter, arraySetter, arrayCreator);
        }

        static public ArrayBindingBuilder<TModel, TView, TModelItem, TNewViewItem> SelectItems<TModel, TView, TModelItem, TOldViewItem, TNewViewItem>(
            this IArrayBindingBuilder<TModel, TView, TModelItem, TOldViewItem> builder,
            Func<TModel, TModelItem, TView, TNewViewItem> itemConverter,
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
        Func<TModel, INotifyArrayChanged> EventSourceGetter { get; set; }
    }

    public interface IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> : IArrayBindingBuilder<TModel, TView>
    {
        Func<TModel, IArray<TModelItem>> ReferenceGetter { get; set; }
        Func<TModel, TModelItem, TView, TViewItem> ItemConverter { get; set; }
        Action<TViewItem> ViewItemDisposer { get; set; }

        void To<TViewArray>(Func<TView, IWriteableArray<TViewItem>> arrayGetter, Action<TView, TViewArray> arraySetter, Func<int[], TViewArray> arrayCreator)
            where TViewArray : IWriteableArray<TViewItem>;
    }

    public class ArrayBindingBuilder<TModel, TView, TModelItem, TViewItem> : IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>
    {
        private readonly ArrayBindingCollection<TModel, TView> _bindingCollection;
        private string _referencePropertyName;
        private Func<TModel, INotifyArrayChanged> _eventSourceGetter;

        private Func<TModel, IArray<TModelItem>> _referenceGetter;
        private Func<TModel, TModelItem, TView, TViewItem> _itemConverter;
        private Action<TViewItem> _viewItemDisposer;

        public ArrayBindingBuilder(
            ArrayBindingCollection<TModel, TView> bindingCollection,
            string referencePropertyName,
            Func<TModel, INotifyArrayChanged> eventSourceGetter,
            Func<TModel, IArray<TModelItem>> referenceGetter,
            Func<TModel, TModelItem, TView, TViewItem> itemConverter
        )
        {
            _bindingCollection = bindingCollection;
            _referencePropertyName = referencePropertyName;
            _referenceGetter = referenceGetter;
            _eventSourceGetter = eventSourceGetter;
            _itemConverter = itemConverter;
        }

        public ArrayBindingBuilder(IArrayBindingBuilder<TModel, TView> builder)
        {
            _bindingCollection = builder.BindingCollection;
            _referencePropertyName = builder.ReferencePropertyName;
            _eventSourceGetter = builder.EventSourceGetter;
        }

        public void To<TViewArray>(Func<TView, IWriteableArray<TViewItem>> arrayGetter, Action<TView, TViewArray> arraySetter, Func<int[], TViewArray> arrayCreator)
            where TViewArray : IWriteableArray<TViewItem>
        {
            _bindingCollection.Add(_referencePropertyName,
                new OneWayArrayBinding<TModel, TView, TModelItem, TViewItem, TViewArray>(
                    _referenceGetter,
                    arrayGetter,
                    arraySetter,
                    arrayCreator,
                    _itemConverter,
                    _viewItemDisposer
                    ).AsEventBinding(_eventSourceGetter));
        }

        #region Explicit

        ArrayBindingCollection<TModel, TView> IArrayBindingBuilder<TModel, TView>.BindingCollection => _bindingCollection;
        string IArrayBindingBuilder<TModel, TView>.ReferencePropertyName
        {
            get => _referencePropertyName;
            set => _referencePropertyName = value;
        }
        Func<TModel, INotifyArrayChanged> IArrayBindingBuilder<TModel, TView>.EventSourceGetter
        {
            get => _eventSourceGetter;
            set => _eventSourceGetter = value;
        }
        Func<TModel, IArray<TModelItem>> IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>.ReferenceGetter
        {
            get => _referenceGetter;
            set => _referenceGetter = value;
        }
        Func<TModel, TModelItem, TView, TViewItem> IArrayBindingBuilder<TModel, TView, TModelItem, TViewItem>.ItemConverter
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
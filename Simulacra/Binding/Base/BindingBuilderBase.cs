using System;

namespace Simulacra.Binding.Base
{
    public abstract class BindingBuilderBase<TModel, TView, TModelValue, TBindingCollection> : IBindingBuilder<TModel, TView, TModelValue, TBindingCollection>
    {
        protected readonly TBindingCollection _bindingCollection;
        protected string _propertyName;
        protected Func<TModel, TView, TModelValue> _getter;

        protected BindingBuilderBase(TBindingCollection bindingCollection, string propertyName, Func<TModel, TView, TModelValue> getter)
        {
            _bindingCollection = bindingCollection;
            _propertyName = propertyName;
            _getter = getter;
        }

        public abstract void Do(Action<TModel, TView> action);
        public abstract IBindingBuilder<TModel, TView, TConvertedValue, TBindingCollection> Select<TConvertedValue>(Func<TModelValue, TModel, TView, TConvertedValue> converter);

        #region Explicit

        Func<TModel, TView, TModelValue> IBindingBuilder<TModel, TView, TModelValue>.Getter
        {
            get => _getter;
            set => _getter = value;
        }

        #endregion
    }
}
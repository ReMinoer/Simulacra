using System.Collections.Generic;
using System.ComponentModel;

namespace Simulacra.Binding.Base
{
    public abstract class ObjectChangingBindingModuleBase<TModel, TView, TBinding> : BindingModuleBase<TModel, TView>
        where TModel : class, INotifyPropertyChanged
        where TView : class
        where TBinding : class, IOneWayBinding<TModel, TView>
    {
        private readonly IReadOnlyDictionary<string, TBinding> _bindings;
        protected override sealed IEnumerable<IOneWayBinding<TModel, TView>> Bindings => _bindings.Values;

        protected ObjectChangingBindingModuleBase(TModel model, IReadOnlyDictionary<string, TBinding> bindings)
            : base(model)
        {
            _bindings = bindings;
        }

        public override void BindView(TView view)
        {
            base.BindView(view);

            foreach (TBinding binding in _bindings.Values)
                BindObject(binding);

            Model.PropertyChanged += OnModelPropertyChanged;
        }

        public override void UnbindView()
        {
            Model.PropertyChanged -= OnModelPropertyChanged;

            UnbindAllObjects();
            base.UnbindView();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_bindings.TryGetValue(e.PropertyName, out TBinding binding))
                return;

            UnbindObject(binding);
            binding.SetView(Model, View);
            BindObject(binding);
        }

        protected abstract void BindObject(TBinding binding);
        protected abstract void UnbindObject(TBinding binding);
        protected abstract void UnbindAllObjects();
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using Simulacra.Binding.Base;

namespace Simulacra.Binding.Property
{
    public class PropertyBindingModule<TModel, TView> : BindingModuleBase<TModel, TView>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        private readonly Dictionary<string, IOneWayBinding<TModel, TView>> _bindings;

        protected override IEnumerable<IOneWayBinding<TModel, TView>> Bindings => _bindings.Values;

        public PropertyBindingModule(TModel model, Dictionary<string, IOneWayBinding<TModel, TView>> bindings)
            : base(model)
        {
            _bindings = bindings;
        }

        public override void BindView(TView view)
        {
            base.BindView(view);
            Model.PropertyChanged += OnModelPropertyChanged;
        }

        public override void UnbindView()
        {
            Model.PropertyChanged -= OnModelPropertyChanged;
            base.UnbindView();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_bindings.TryGetValue(e.PropertyName, out IOneWayBinding<TModel, TView> binding))
                binding.SetView(Model, View);
        }
    }
}
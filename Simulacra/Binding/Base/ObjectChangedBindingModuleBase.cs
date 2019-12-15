using System.Collections.Generic;
using System.ComponentModel;

namespace Simulacra.Binding.Base
{
    public abstract class ObjectChangedBindingModuleBase<TModel, TView, TEventArgs, TEventSource> : BindingModuleBase<TModel, TView>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        private readonly Dictionary<string, IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource>> _bindings;
        private readonly Dictionary<object, List<IOneWayBinding<TModel, TView, TEventArgs>>> _bindedObjects;

        protected override IEnumerable<IOneWayBinding<TModel, TView>> Bindings => _bindings.Values;

        protected ObjectChangedBindingModuleBase(TModel model, Dictionary<string, IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource>> bindings)
            : base(model)
        {
            _bindings = bindings;
            _bindedObjects = new Dictionary<object, List<IOneWayBinding<TModel, TView, TEventArgs>>>();
        }

        protected abstract void Subscribe(TEventSource eventSource);
        protected abstract void Unsubscribe(TEventSource eventSource);

        public override void BindView(TView view)
        {
            base.BindView(view);

            Model.PropertyChanged += OnModelPropertyChanged;

            foreach (IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource> binding in _bindings.Values)
                BindObject(binding);
        }

        public override void UnbindView()
        {
            Model.PropertyChanged -= OnModelPropertyChanged;

            foreach (KeyValuePair<object, List<IOneWayBinding<TModel, TView, TEventArgs>>> bindingEntry in _bindedObjects)
            {
                var obj = (TEventSource)bindingEntry.Key;
                List<IOneWayBinding<TModel, TView, TEventArgs>> bindingList = bindingEntry.Value;

                for (int i = 0; i < bindingList.Count; i++)
                    Unsubscribe(obj);
            }

            _bindedObjects.Clear();
            base.UnbindView();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_bindings.TryGetValue(e.PropertyName, out IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource> binding))
                return;

            UnbindObject(binding);
            binding.SetView(Model, View);
            BindObject(binding);
        }

        protected void OnModelObjectChanged(object sender, TEventArgs e)
        {
            if (!_bindedObjects.TryGetValue(sender, out List<IOneWayBinding<TModel, TView, TEventArgs>> bindingList))
                return;

            foreach (IOneWayBinding<TModel, TView, TEventArgs> binding in bindingList)
                binding.UpdateView(Model, View, e);
        }

        private void BindObject(IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource> binding)
        {
            TEventSource obj = binding.GetModelEventSource(Model);
            if (!_bindedObjects.TryGetValue(obj, out List<IOneWayBinding<TModel, TView, TEventArgs>> bindingList))
                _bindedObjects[obj] = bindingList = new List<IOneWayBinding<TModel, TView, TEventArgs>>();

            bindingList.Add(binding);
            Subscribe(obj);
        }

        private void UnbindObject(IOneWayEventBinding<TModel, TView, TEventArgs, TEventSource> binding)
        {
            TEventSource obj = binding.GetModelEventSource(Model);
            if (!_bindedObjects.TryGetValue(obj, out List<IOneWayBinding<TModel, TView, TEventArgs>> bindingList))
                return;
            
            Unsubscribe(obj);
            bindingList.Remove(binding);

            if (bindingList.Count == 0)
                _bindedObjects.Remove(obj);
        }
    }
}
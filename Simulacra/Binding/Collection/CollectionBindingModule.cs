using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Simulacra.Binding.Collection
{
    public class CollectionBindingModule<TModel, TView> : BindingModuleBase<TModel, TView>
        where TModel : class, INotifyPropertyChanged
        where TView : class
    {
        private readonly Dictionary<string, IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged>> _bindings;
        private readonly Dictionary<object, List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>>> _bindedCollections;

        protected override IEnumerable<IOneWayBinding<TModel, TView>> Bindings => _bindings.Values;

        public CollectionBindingModule(TModel model, Dictionary<string, IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged>> bindings)
            : base(model)
        {
            _bindings = bindings;
            _bindedCollections = new Dictionary<object, List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>>>();
        }

        public override void BindView(TView view)
        {
            base.BindView(view);
            
            Model.PropertyChanged += OnModelPropertyChanged;

            foreach (IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> binding in _bindings.Values)
                BindCollection(binding);
        }

        public override void UnbindView()
        {
            Model.PropertyChanged -= OnModelPropertyChanged;

            foreach (KeyValuePair<object, List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>>> bindingEntry in _bindedCollections)
            {
                var collection = (INotifyCollectionChanged)bindingEntry.Key;
                List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>> bindingList = bindingEntry.Value;

                for (int i = 0; i < bindingList.Count; i++)
                    collection.CollectionChanged -= OnModelCollectionChanged;
            }

            _bindedCollections.Clear();
            base.UnbindView();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_bindings.TryGetValue(e.PropertyName, out IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> binding))
                return;

            UnbindCollection(binding);
            binding.SetView(Model, View);
            BindCollection(binding);
        }

        private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_bindedCollections.TryGetValue(sender, out List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>> bindingList))
                return;

            foreach (IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs> binding in bindingList)
                binding.UpdateView(Model, View, e);
        }

        private void BindCollection(IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> binding)
        {
            INotifyCollectionChanged collection = binding.GetModelEventSource(Model);
            if (!_bindedCollections.TryGetValue(collection, out List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>> bindingList))
                _bindedCollections[collection] = bindingList = new List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>>();
            
            bindingList.Add(binding);
            collection.CollectionChanged += OnModelCollectionChanged;
        }

        private void UnbindCollection(IOneWayEventBinding<TModel, TView, NotifyCollectionChangedEventArgs, INotifyCollectionChanged> binding)
        {
            INotifyCollectionChanged collection = binding.GetModelEventSource(Model);
            if (!_bindedCollections.TryGetValue(collection, out List<IOneWayBinding<TModel, TView, NotifyCollectionChangedEventArgs>> bindingList))
                return;
        
            collection.CollectionChanged -= OnModelCollectionChanged;

            bindingList.Remove(binding);
            if (bindingList.Count == 0)
                _bindedCollections.Remove(collection);
        }
    }
}
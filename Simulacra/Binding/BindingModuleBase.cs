using System.Collections.Generic;

namespace Simulacra.Binding
{
    public abstract class BindingModuleBase<TModel, TView> : IBindingModule<TView>
        where TModel : class
        where TView : class
    {
        protected TModel Model { get; }
        protected TView View { get; private set; }
        protected abstract IEnumerable<IOneWayBinding<TModel, TView>> Bindings { get; }

        protected BindingModuleBase(TModel model)
        {
            Model = model;
        }

        public virtual void InitializeView(TView view)
        {
            foreach (IOneWayBinding<TModel, TView> binding in Bindings)
                binding.SetView(Model, view);
        }

        public virtual void BindView(TView view)
        {
            View = view;
        }

        public virtual void UnbindView()
        {
            View = null;
        }
    }
}
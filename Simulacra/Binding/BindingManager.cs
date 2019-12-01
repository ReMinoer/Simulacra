using System.Collections.Generic;

namespace Simulacra.Binding
{
    public class BindingManager<TView>
    {
        public List<IBindingModule<TView>> Modules { get; } = new List<IBindingModule<TView>>();
        
        public void InitializeView(TView view)
        {
            foreach (IBindingModule<TView> module in Modules)
                module.InitializeView(view);
        }

        public void BindView(TView view)
        {
            foreach (IBindingModule<TView> module in Modules)
                module.BindView(view);
        }

        public void UnbindView()
        {
            foreach (IBindingModule<TView> module in Modules)
                module.UnbindView();
        }
    }
}
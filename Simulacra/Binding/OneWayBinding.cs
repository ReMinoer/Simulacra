using System;

namespace Simulacra.Binding
{
    public class OneWayBinding<TModel, TView> : IOneWayBinding<TModel, TView>
    {
        private readonly Action<TModel, TView> _setter;

        public OneWayBinding(Action<TModel, TView> setter)
        {
            _setter = setter;
        }
        
        public void SetView(TModel model, TView view) => _setter(model, view);
    }
}
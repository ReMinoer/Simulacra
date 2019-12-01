namespace Simulacra.Binding
{
    public class TwoWayBinding<TModel, TView> : ITwoWayBinding<TModel, TView>
    {
        private readonly IOneWayBinding<TModel, TView> _binding;
        private readonly IOneWayBinding<TView, TModel> _reverseBinding;

        public TwoWayBinding(IOneWayBinding<TModel, TView> binding, IOneWayBinding<TView, TModel> reverseBinding)
        {
            _binding = binding;
            _reverseBinding = reverseBinding;
        }

        public void SetView(TModel model, TView view) => _binding.SetView(model, view);
        public void InitializeModel(TModel model, TView view) => _reverseBinding.SetView(view, model);
    }

    public class TwoWayBinding<TModel, TView, TNotification> : TwoWayBinding<TModel, TView>, ITwoWayBinding<TModel, TView, TNotification>
    {
        private readonly IOneWayBinding<TModel, TView, TNotification> _binding; 
        private readonly IOneWayBinding<TView, TModel, TNotification> _reverseBinding;

        public TwoWayBinding(IOneWayBinding<TModel, TView, TNotification> binding, IOneWayBinding<TView, TModel, TNotification> reverseBinding)
            : base(binding, reverseBinding)
        {
            _binding = binding;
            _reverseBinding = reverseBinding;
        }
        
        public void UpdateView(TModel model, TView view, TNotification notification) => _binding.UpdateView(model, view, notification);
        public void UpdateModel(TModel model, TView view, TNotification notification) => _reverseBinding.UpdateView(view, model, notification);
    }
}
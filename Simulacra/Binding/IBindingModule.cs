namespace Simulacra.Binding
{
    public interface IBindingModule<in TView>
    {
        void InitializeView(TView view);
        void BindView(TView view);
        void UnbindView();
    }
}
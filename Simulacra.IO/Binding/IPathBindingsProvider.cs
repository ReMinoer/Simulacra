namespace Simulacra.IO.Binding
{
    public interface IPathBindingsProvider<TModel, TView>
    {
        PathBindingCollection<TModel, TView> PathBindings { get; }
    }
}
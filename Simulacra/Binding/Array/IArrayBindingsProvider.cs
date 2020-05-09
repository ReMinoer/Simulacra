namespace Simulacra.Binding.Array
{
    public interface IArrayBindingsProvider<TModel, TView>
    {
        ArrayBindingCollection<TModel, TView> ArrayBindings { get; }
    }
}
namespace Simulacra.Binding.Collection
{
    public interface ICollectionBindingsProvider<TModel, TView>
    {
        CollectionBindingCollection<TModel, TView> CollectionBindings { get; }
    }
}
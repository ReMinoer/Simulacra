namespace Simulacra.Binding.Property
{
    public interface IPropertyBindingsProvider<TModel, TView>
    {
        PropertyBindingCollection<TModel, TView> PropertyBindings { get; }
    }
}
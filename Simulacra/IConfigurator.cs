namespace Simulacra
{
    public interface IConfigurator<in T>
    {
        void Configure(T obj);
    }
}
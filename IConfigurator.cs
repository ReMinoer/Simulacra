namespace Diese.Modelization
{
    public interface IConfigurator<in T>
    {
        void Configure(T obj);
    }
}
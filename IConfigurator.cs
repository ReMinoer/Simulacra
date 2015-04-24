namespace Diese.Modelization
{
    public interface IConfigurator<in T> : IDataModel<T>
    {
        void Configure(T obj);
    }
}
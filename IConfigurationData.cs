namespace Diese.Modelization
{
    public interface IConfigurationData<in T> : IDataModel<T>, IConfigurator<T>
    {
    }
}
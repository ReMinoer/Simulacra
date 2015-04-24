namespace Diese.Modelization
{
    public interface ICreator<T> : IDataModel<T>
    {
        T Create();
    }
}
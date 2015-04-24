namespace Diese.Modelization
{
    public interface IDataModel<in T>
    {
        void From(T obj);
    }
}
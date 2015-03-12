namespace Diese.Modelization
{
    public interface IDataModel<T>
    {
        void From(T obj);
        void To(T obj);
        T Create();
    }
}
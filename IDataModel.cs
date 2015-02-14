namespace Diese.Modelization
{
    public interface IDataModel<T>
    {
        void From(T obj);
        void To(out T obj);
    }
}
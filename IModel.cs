namespace Diese.Modelization
{
    public interface IModel<in T>
    {
        void From(T obj);
        void To(T obj);
    }
}
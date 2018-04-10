namespace Simulacra
{
    public interface IDataModel<in T>
    {
        void From(T obj);
    }
}
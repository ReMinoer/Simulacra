namespace Diese.Modelization
{
    public interface ICreator<out T>
    {
        T Create();
    }
}
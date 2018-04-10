namespace Simulacra
{
    public interface ICreator<out T>
    {
        T Create();
    }
}
namespace Simulacra.Utils
{
    public interface IArrayDefinition
    {
        int Rank { get; }
        int GetLowerBound(int dimension);
        int GetLength(int dimension);
    }
}
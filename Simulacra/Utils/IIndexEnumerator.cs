namespace Simulacra.Utils
{
    public interface IIndexEnumerator
    {
        int[] GetResetIndex();
        bool MoveIndex(int[] indexes);
    }
}
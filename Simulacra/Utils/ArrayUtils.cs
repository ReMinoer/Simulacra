namespace Simulacra.Utils
{
    public class ArrayUtils
    {
        static public int[] GetResetIndex(IArrayDefinition array)
        {
            int[] indexes = new int[array.Rank];
            indexes[array.Rank - 1]--;
            return indexes;
        }

        static public bool MoveIndex(IArrayDefinition array, int[] indexes)
        {
            indexes[array.Rank - 1]++;

            for (int r = array.Rank - 1; r >= 0; r--)
            {
                if (indexes[r] < array.GetLength(r))
                    break;

                if (r - 1 < 0)
                    return false;

                indexes[r] = 0;
                indexes[r - 1]++;
            }

            return true;
        }
    }
}
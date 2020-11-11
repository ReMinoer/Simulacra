using System;

namespace Simulacra.Utils
{
    public class ArrayDefinition : IArrayDefinition
    {
        private readonly Array _array;

        public ArrayDefinition(Array array)
        {
            _array = array;
        }

        public int Rank => _array.Rank;
        public int GetLowerBound(int dimension) => _array.GetLowerBound(dimension);
        public int GetLength(int dimension) => _array.GetLength(dimension);
    }
}
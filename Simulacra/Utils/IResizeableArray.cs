using System;

namespace Simulacra.Utils
{
    public interface IResizeableArrayDefinition : IArrayDefinition
    {
        int[] Lengths { set; }
    }

    public interface IResizeableArray : IResizeableArrayDefinition, IArray
    {
    }

    public interface IOneDimensionResizeableArray : IResizeableArray, IOneDimensionArray
    {
    }

    public interface ITwoDimensionResizeableArray : IResizeableArray, ITwoDimensionArray
    {
    }

    public interface IThreeDimensionResizeableArray : IResizeableArray, IThreeDimensionArray
    {
    }

    public interface IFourDimensionResizeableArray : IResizeableArray, IFourDimensionArray
    {
    }

    public interface IResizeableArray<T> : IResizeableArray, IWriteableArray<T>
    {
        void Resize(int[] newLengths, bool keepValues = true, Func<T, int[], T> valueFactory = null);
    }

    public interface IOneDimensionResizeableArray<T> : IOneDimensionResizeableArray, IResizeableArray<T>, IOneDimensionWriteableArray<T>
    {
    }

    public interface ITwoDimensionResizeableArray<T> : ITwoDimensionResizeableArray, IResizeableArray<T>, ITwoDimensionWriteableArray<T>
    {
    }

    public interface IThreeDimensionResizeableArray<T> : IThreeDimensionResizeableArray, IResizeableArray<T>, IThreeDimensionWriteableArray<T>
    {
    }

    public interface IFourDimensionResizeableArray<T> : IFourDimensionResizeableArray, IResizeableArray<T>, IFourDimensionWriteableArray<T>
    {
    }
}
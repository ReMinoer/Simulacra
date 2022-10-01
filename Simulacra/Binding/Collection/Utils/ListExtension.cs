using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Binding.Collection.Utils
{
    static public class ListExtension
    {
        static public void InsertMany<T>(this IList<T> list, int index, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Insert(index, item);
                index++;
            }
        }

        static public void ReplaceRange<T>(this IList<T> list, int index, IEnumerable<T> newItems)
        {
            foreach (T newItem in newItems)
            {
                list.RemoveAt(index);
                list.Insert(index, newItem);
                index++;
            }
        }

        static public bool MoveMany<T>(this IList<T> list, IEnumerable<T> items, int index)
        {
            T[] itemsArray = items.ToArray();
            int[] oldIndices = itemsArray.Select(list.IndexOf).OrderByDescending(x => x).ToArray();
            if (oldIndices.Any(x => x == -1))
                return false;

            foreach (int oldIndex in oldIndices)
                list.RemoveAt(oldIndex);

            list.InsertMany(index, itemsArray);
            return true;
        }
    }
}
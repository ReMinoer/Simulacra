using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Simulacra.Binding.Collection.Utils
{
    static public class ListExtension
    {
        static public void InsertMany<T>(this IList<T> list, int index, IEnumerable<T> items)
        {
            foreach (T item in items)
                list.Insert(index++, item);
        }

        static public void ReplaceRange<T>(this IList<T> list, int oldStartingIndex, int newStartingIndex, IEnumerable<T> newItems)
        {
            foreach (T newItem in newItems)
            {
                list.RemoveAt(oldStartingIndex++);
                list.Insert(newStartingIndex++, newItem);
            }
        }

        static public bool MoveMany<T>(this IList<T> list, IEnumerable<T> items, int newIndex)
        {
            T[] itemsArray = items.ToArray();
            List<int> indexes = itemsArray.Select(list.IndexOf).ToList();
            if (indexes.Any(x => x == -1))
                return false;

            indexes.Sort(Comparer<int>.Create((x, y) => y - x));

            foreach (int index in indexes)
                list.RemoveAt(index);

            list.InsertMany(newIndex, itemsArray);
            return true;
        }
    }
}
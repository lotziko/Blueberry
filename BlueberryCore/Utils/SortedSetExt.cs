using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public static class SortedSetExt
    {
        /// <summary>
		/// returns false if the item is already in the List and true if it was successfully added.
		/// </summary>
		/// <returns>The if not present.</returns>
		/// <param name="list">List.</param>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool AddIfNotPresent<T>(this SortedSet<T> list, T item)
        {
            if (list.Contains(item))
                return false;

            list.Add(item);
            return true;
        }

        public static void Add<T>(this SortedSet<T> list, SortedSet<T> set)
        {
            using (var sequenceEnum = set.GetEnumerator())
            {
                while (sequenceEnum.MoveNext())
                {
                    list.Add(sequenceEnum.Current);
                }
            }
        }

        public static void AddRange<T>(this SortedSet<T> list, IEnumerable<T> collection)
        {
            using (var sequenceEnum = collection.GetEnumerator())
            {
                while (sequenceEnum.MoveNext())
                {
                    list.Add(sequenceEnum.Current);
                }
            }
        }
    }
}

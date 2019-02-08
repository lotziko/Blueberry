using System.Collections.Generic;

namespace Blueberry
{
    public static class ListExt
    {
        /// <summary>
		/// returns false if the item is already in the List and true if it was successfully added.
		/// </summary>
		/// <returns>The if not present.</returns>
		/// <param name="list">List.</param>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool AddIfNotPresent<T>(this IList<T> list, T item)
        {
            if (list.Contains(item))
                return false;

            list.Add(item);
            return true;
        }

        public static T Peek<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        public static T Pop<T>(this IList<T> list)
        {
            T item = default;
            int count = list.Count;
            if (count > 1)
                item = list[count - 2];
            list.RemoveAt(list.Count - 1);
            return item;
        }
    }
}

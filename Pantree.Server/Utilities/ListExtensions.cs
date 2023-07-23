using System.Collections.Generic;
using System.Linq;

namespace Pantree.Server.Utilities
{
    /// <summary>
    /// A collection of extension methods for <see cref="List{T}"/> instances
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Add an <paramref name="item"/> to a given list, creating the list if needed
        /// </summary>
        /// <param name="list">The list to add the item to; if null, a new list will be instantiated</param>
        /// <param name="item">The item to add</param>
        /// <typeparam name="T">The type of item contained in the list</typeparam>
        /// <returns>The list with the item added</returns>
        public static List<T> CreateOrAdd<T>(this List<T>? list, T item)
        {
            list ??= new();
            list.Add(item);
            return list;
        }

        /// <summary>
        /// Add <paramref name="items"/> to a given list, creating the list if needed
        /// </summary>
        /// <param name="list">The list to add the items to; if null, a new list will be instantiated</param>
        /// <param name="items">The items to add</param>
        /// <typeparam name="T">The type of item contained in the list</typeparam>
        /// <returns>The list with the items added</returns>
        public static List<T> CreateOrAddRange<T>(this List<T>? list, IEnumerable<T> items)
        {
            list ??= new();
            list.AddRange(items);
            return list;
        }

        /// <summary>
        /// Check if this list equals another list without asserting the order of the elements within the lists
        /// </summary>
        /// <remarks>
        /// This is similar to <see cref="Enumerable.SequenceEqual{T}(IEnumerable{T}, IEnumerable{T})"/>, except that
        /// the order of the elements is not relied upon.
        /// </remarks>
        /// <param name="lhs">The first list</param>
        /// <param name="rhs">The second list</param>
        /// <typeparam name="T">The type of item contained in the list</typeparam>
        /// <returns>
        /// True if the lists have the same length and contain equal elements (in any order), false otherwise
        /// </returns>
        public static bool SequenceEqualUnordered<T>(this List<T>? lhs, List<T>? rhs)
        {
            if (lhs is null)
                return rhs is null;
            if (rhs is null)
                return false;

            if (lhs.Count != rhs.Count)
                return false;

            bool isEqual = true;
            foreach (T item in lhs)
                isEqual = isEqual && rhs.Any(x => 
                {
                    if (item is null)
                        return x is null;
                    return item.Equals(x);
                });

            return isEqual;
        }
    }
}

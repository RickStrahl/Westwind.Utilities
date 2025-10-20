using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Utilities
{
    public static class GenericUtils
    {
        /// <summary>
        /// Checks if an item is contained in a given list of values.
        /// Equivalent to SQL's IN clause.
        /// </summary>
        public static bool InList<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Overload that takes any IEnumerable&lt;&lt;.
        /// </summary>
        public static bool InList<T>(this T item, IEnumerable<T> list)
        {
            return list.Contains(item);
        }


        /// <summary>
        /// Determines whether an item is contained in a list of other items
        /// </summary>
        /// <example>
        /// bool exists = Inlist&lt;string&gt;("Rick","Mike","Billy","Rick","Frank"); // true;
        /// </example>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="item">The item to look for</param>
        /// <param name="list">Any number of items to search (params)</param>
        /// <returns></returns>
        public static bool Inlist<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }


        /// <summary>
        /// A string is contained in a list of strings
        /// </summary>
        /// <param name="item">string to look for</param>
        /// <param name="list">list of strings</param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public static bool Inlist(this string item,  bool caseSensitive, params string[] list)
        {
            if (caseSensitive)
                return list.Contains(item);

            foreach (var listItem in list)
            {
                if (listItem.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}

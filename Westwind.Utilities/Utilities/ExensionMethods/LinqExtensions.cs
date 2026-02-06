using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Utilities.Linq
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Recursively flattens a tree structure of nested same type enumerables
        /// into a flat structure. 
        /// 
        /// Example:
        /// Flattening a tree of documentation topics into a flat list of topics.
        /// </summary>
        /// <typeparam name="T">Type to flatten</typeparam>
        /// <param name="e">Enumeration to work on</param>
        /// <param name="f">Expression that points at the element list to select from</param>
        /// <returns>Flattened list or empty enumerable</returns>
        /// <example>
        /// var topics = topicTree.FlattenTree(t=&gt; t.Topics);
        /// </example>
        public static IEnumerable<T> FlattenTree<T>(
            this IEnumerable<T> e,
            Func<T, IEnumerable<T>> f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            var items = (e ?? Enumerable.Empty<T>()).ToList();

            return items
                .SelectMany(c => (f(c) ?? Enumerable.Empty<T>()).FlattenTree(f))
                .Concat(items);
        }
    }
}

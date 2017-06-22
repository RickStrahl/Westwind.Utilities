using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Utilities.Linq
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Flattens a tree to a plain enumerable for querying.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">Enumeration to work on</param>
        /// <param name="f">Expression that points at the element list to select from</param>
        /// <returns></returns>
        /// <example>
        /// var topics = topicTree.FlattenTree(t=> t.Topics);
        /// </example>
        public static IEnumerable<T> FlattenTree<T>(
            this IEnumerable<T> e,
            Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).FlattenTree(f)).Concat(e);
        }
    }
}

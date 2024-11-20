using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Groups LINQ extensions
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Separate a list into N subsets
        /// </summary>
        /// <typeparam name="T">the type of the list</typeparam>
        /// <param name="items">the list to separate</param>
        /// <param name="partitionSize">the size 'N' of the subsets</param>
        /// <returns>the list of subsets of N elements (except the last)</returns>
        public static IEnumerable<T[]> SplitInChunksOf<T>(this IEnumerable<T> items, int partitionSize)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (partitionSize < 1)
                throw new ArgumentException(nameof(partitionSize));

            var res = new T[partitionSize];
            var i = 0;
            foreach (var item in items)
            {
                res[i] = item;
                i++;

                if (i != partitionSize) continue;
                yield return res;
                res = new T[partitionSize];
                i = 0;
            }
            // remaining
            if (i <= 0) yield break;
            if (i == partitionSize)
            {
                yield return res;
            }
            else
            {
                var tempItems = new T[i];
                Array.Copy(res, tempItems, i);
                yield return tempItems;
            }
        }
        /// <summary>
        /// Returns the next element of a list (taking the first if it was the last)
        /// If there are less than two elements, we return the current element
        /// </summary>
        /// <typeparam name="T">the type of the list</typeparam>
        /// <param name="items">The list </param>
        /// <param name="current">the current element</param>
        /// <returns>the next element in the list</returns>
        public static T Next<T>(this IList<T> items, T current)
        {
            return items.Count < 2
                ? current
                : items[(items.IndexOf(current) + 1) % items.Count];
        }
    }
}
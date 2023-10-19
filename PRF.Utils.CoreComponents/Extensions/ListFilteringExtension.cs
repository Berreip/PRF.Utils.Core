using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBePrivate.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extensions methods for list filtering
    /// </summary>
    public static class ListFilteringExtension
    {
        /// <summary>
        /// Filter a list up to the given number of element by removing at random some element. WARNING: the list itself is filtered, not a copy
        /// </summary>
        public static void CapRandomized<T>(this List<T> listToFilter, int maxItemTarget)
        {
            listToFilter.CapRandomized(maxItemTarget, new Random());
        }

        /// <summary>
        /// Filter a list up to the given number of element by removing at random some element. WARNING: the list itself is filtered, not a copy
        /// </summary>
        public static void CapRandomized<T>(this List<T> listToFilter, int maxItemTarget, Random random)
        {
            if (maxItemTarget > listToFilter.Count)
            {
                throw new ArgumentException("RandomCapList: the list is smaller than the maximum number of items so we should not have a filter in this case");
            }
            if (maxItemTarget == listToFilter.Count || listToFilter.Count == 0)
            {
                return;
            }

            while (listToFilter.Count > maxItemTarget)
            {
                listToFilter.RemoveAt(random.Next(0, listToFilter.Count - 1));
            }
        }
        /// <summary>
        /// return a random element from the list
        /// Returns an element drawn randomly from the list
        /// </summary>
        public static T GetRandomElement<T>(this IReadOnlyList<T> list, Random random)
        {
            return list.Count == 0
                ? default
                : list[random.Next(0, list.Count - 1)];
        }

    }
}

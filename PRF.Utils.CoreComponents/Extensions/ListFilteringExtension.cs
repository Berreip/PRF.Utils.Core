using System;
using System.Collections.Generic;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extensions methods for list filtering
    /// </summary>
    public static class ListFilteringExtension
    {
        /// <summary>
        /// Filter a list up to the given number of element by removing at raandom some element. WARNING: the list itself is filtered, not a copy
        /// </summary>
        public static void CapRandomized<T>(this List<T> listToFilter, int maxItemTarget)
        {
            listToFilter.CapRandomized(maxItemTarget, new Random());
        }

        /// <summary>
        /// Filter a list up to the given number of element by removing at raandom some element. WARNING: the list itself is filtered, not a copy
        /// </summary>
        public static void CapRandomized<T>(this List<T> listToFilter, int maxItemTarget, Random random)
        {
            if (maxItemTarget > listToFilter.Count)
            {
                throw new ArgumentException("RandomCapList : la liste est plus petite que le nombre d'item maximum donc on ne devrait pas avoir de filtre dans ce cas");
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
        /// Renvoie un élément tiré au hasard de la liste
        /// </summary>
        public static T GetRandomElement<T>(this IReadOnlyList<T> list, Random random)
        {
            return list.Count == 0
                ? default
                : list[random.Next(0, list.Count - 1)];
        }

    }
}

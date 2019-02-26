using System;
using System.Collections.Generic;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Module d'extension de filtre de listes
    /// </summary>
    public static class ListFilteringExtension
    {
        /// <summary>
        /// A partir d'une liste de N éléments et un nombre d'éléments cible C où N >= C
        /// on filtre jusqu'à obtenir le nombre d'éléments voulus dans la liste en tirant au hasard les élément à supprimer
        ///  -> ATTENTION: la liste elle même est filtrée, on n'en renvoie pas une copie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToFilter">la liste à filtrer</param>
        /// <param name="maxItemTarget">le nombre d'élément cible</param>
        public static void CapRandomized<T>(this List<T> listToFilter, int maxItemTarget)
        {
            listToFilter.CapRandomized(maxItemTarget, new Random());
        }

        /// <summary>
        /// A partir d'une liste de N éléments et un nombre d'éléments cible C où N >= C
        /// on filtre jusqu'à obtenir le nombre d'éléments voulus dans la liste en tirant au hasard les élément à supprimer
        ///  -> ATTENTION: la liste elle même est filtrée, on n'en renvoie pas une copie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToFilter">la liste à filtrer</param>
        /// <param name="maxItemTarget">le nombre d'élément cible</param>
        /// <param name="random">le seed pour la randomisation (optionnel)</param>
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

            // tant que la liste est plus grande que la cible, on tire au sort un élément à retirer
            while (listToFilter.Count > maxItemTarget)
            {
                listToFilter.RemoveAt(random.Next(0, listToFilter.Count - 1));
            }
        }

        /// <summary>
        /// Renvoie un élément tiré au hasard de la liste
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">la liste à filtrer</param>
        /// <param name="random">le seed pour la randomisation (optionnel)</param>
        public static T GetRandomElement<T>(this List<T> list, Random random)
        {
            return list.Count == 0
                ? default(T)
                : list[random.Next(0, list.Count - 1)];
        }

    }
}

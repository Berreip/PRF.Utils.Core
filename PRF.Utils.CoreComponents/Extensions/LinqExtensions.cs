﻿using System;
using System.Collections.Generic;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Regroupe les extensions LINQ
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Sépare une liste en N sous-ensembles
        /// </summary>
        /// <typeparam name="T">le type de la liste</typeparam>
        /// <param name="items">la liste à séparer</param>
        /// <param name="partitionSize">la taille 'N' des sous ensembles</param>
        /// <returns>la liste de sous ensemble de N éléments (sauf le dernier)</returns>
        public static IEnumerable<IEnumerable<T>> SplitInChunckOf<T>(this IEnumerable<T> items, int partitionSize)
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
            // les reste
            if (i <= 0) yield break;
            if (i == partitionSize)
                yield return res;
            else
            {
                var tempItems = new T[i];
                Array.Copy(res, tempItems, i);
                yield return tempItems;
            }
        }

        /// <summary>
        /// Renvoie le prochain élément d'une liste (en reprenant le premier s'il s'agissait du dernier)
        /// S'il y a moins de deux éléments, on renvoie l'élément actuel
        /// </summary>
        /// <typeparam name="T">le type de la liste</typeparam>
        /// <param name="items">La liste </param>
        /// <param name="current">l'élément actuel</param>
        /// <returns>le prochain élément de la liste</returns>
        public static T Next<T>(this IList<T> items, T current)
        {
            return items.Count < 2
                ? current
                : items[(items.IndexOf(current) + 1) % items.Count];
        }

        /// <summary>
        /// Génère un hashSet à partir de la liste (attention, s'il y a des doublons ils seront effacés)
        /// => peut éventuellement servir à filtrer les doublons du coup.
        /// </summary>
        /// <typeparam name="T">le type de la liste</typeparam>
        /// <param name="items">la liste à extraire</param>
        /// <param name="throwExceptionOnDuplicate">paramètre optionnel qui détermine si l'on lance une exception en cas de 
        /// doublons dans la liste à extraire</param>
        /// <returns>le hashSet généré</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, bool throwExceptionOnDuplicate = false)
        {
            var hash = new HashSet<T>();
            foreach (var item in items)
            {
                if (!hash.Add(item) && throwExceptionOnDuplicate)
                {
                    throw new ArgumentException($"méthode ToHashSet: l'élément {item} est déjà présent dans le hashset et le paramètre optionnel 'throwExceptionOnDuplicate' n'autorise pas les duplicats (true)");
                }
            }
            return hash;
        }
    }
}
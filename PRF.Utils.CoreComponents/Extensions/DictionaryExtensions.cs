using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Méthodes d'extensions pour les Dictionnaires et Dictionnaires Concurentiels
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Ajoute les éléments demandés en gérant le différentiel (les éléments manquants sont ajoutés,
        ///  les éléments en trop sont retirés, les autres sont laissés tel quel)
        /// ATTENTION, on compare les éléments par clé et on remplace les valeurs.
        /// La comparaison se base sur les égalités des types T1. Il faut donc bien penser à surcharger les opérateurs
        /// d'égalité pour obtenir un comportement spécifique
        /// </summary>
        /// <param name="dictionary">Le dictionnaire où l'on souhaite rajouter des éléments</param>
        /// <param name="elementsToAdd">la liste des éléments à rajouter</param>
        public static void AddRangeDifferential<T1, T2>(this Dictionary<T1, T2> dictionary, IEnumerable<KeyValuePair<T1, T2>> elementsToAdd)
        {
            var elementsPresent = new HashSet<T1>();
            // parcours la liste d'éléments à ajouter une seule et unique fois
            foreach (var keyValuePair in elementsToAdd)
            {
                if (!dictionary.ContainsKey(keyValuePair.Key))
                {
                    // sinon on ajoute
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }
                // on signale l'élément comme présent
                elementsPresent.Add(keyValuePair.Key);
            }

            // puis on retire les éléments non présents:
            foreach (var res in dictionary.Where(o => !elementsPresent.Contains(o.Key)).ToList())
            {
                dictionary.Remove(res.Key);
            }
        }


        /// <summary>
        /// Ajoute les éléments demandés en gérant le différentiel (les éléments manquants sont ajoutés,
        ///  les éléments en trop sont retirés, les autres sont laissés tel quel)
        /// ATTENTION, on compare les éléments par clé et on remplace les valeurs.
        /// La comparaison se base sur les égalités des types T1. Il faut donc bien penser à surcharger les opérateurs
        /// d'égalité pour obtenir un comportement spécifique
        /// </summary>
        /// <param name="dictionary">Le dictionnaire où l'on souhaite rajouter des éléments</param>
        /// <param name="elementsToAdd">la liste des éléments à rajouter</param>
        public static void AddRangeDifferential<T1, T2>(this ConcurrentDictionary<T1, T2> dictionary, IEnumerable<KeyValuePair<T1, T2>> elementsToAdd)
        {
            var elementsPresent = new HashSet<T1>();
            // parcours la liste d'éléments à ajouter une seule et unique fois
            foreach (var keyValuePair in elementsToAdd)
            {
                // en cas de clé trouvé, l'update de fait que reprendre l'ancien élément
                dictionary.AddOrUpdate(keyValuePair.Key, keyValuePair.Value, (key, value) => value);

                // on signale l'élément comme présent
                elementsPresent.Add(keyValuePair.Key);
            }

            // puis on retire les éléments non présents:
            foreach (var res in dictionary.Where(o => !elementsPresent.Contains(o.Key)).ToList())
            {
                dictionary.TryRemove(res.Key, out _);
            }
        }
    }
}

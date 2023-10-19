using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension methods for Dictionaries and Concurrent Dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds the requested elements by managing the differential (missing elements are added,
        /// excess elements are removed, the others are left as is)
        /// ATTENTION, we compare the elements by key and we replace the values.
        /// The comparison is based on the equalities of the T1 types. We must therefore think carefully about overloading the operators
        /// equality to obtain a specific behavior
        /// </summary>
        /// <param name="dictionary">The dictionary where we wish to add elements</param>
        /// <param name="elementsToAdd">the list of elements to add</param>
        public static void AddRangeDifferential<T1, T2>(this Dictionary<T1, T2> dictionary, IEnumerable<KeyValuePair<T1, T2>> elementsToAdd)
        {
            var elementsPresent = new HashSet<T1>();
            // scan the list of elements to add once and only once
            foreach (var keyValuePair in elementsToAdd)
            {
                if (!dictionary.ContainsKey(keyValuePair.Key))
                {
                    // otherwise we add
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }

                // we report the element as present
                elementsPresent.Add(keyValuePair.Key);
            }

            // then we remove the elements not present:
            foreach (var res in dictionary.Where(o => !elementsPresent.Contains(o.Key)).ToList())
            {
                dictionary.Remove(res.Key);
            }
        }

        /// <summary>
        /// Adds the requested elements by managing the differential (missing elements are added,
        /// excess elements are removed, the others are left as is)
        /// ATTENTION, we compare the elements by key and we replace the values.
        /// The comparison is based on the equalities of the T1 types. We must therefore think carefully about overloading the operators
        /// equality to obtain a specific behavior
        /// </summary>
        /// <param name="dictionary">The dictionary where we wish to add elements</param>
        /// <param name="elementsToAdd">the list of elements to add</param>
        public static void AddRangeDifferential<T1, T2>(this ConcurrentDictionary<T1, T2> dictionary, IEnumerable<KeyValuePair<T1, T2>> elementsToAdd)
        {
            var elementsPresent = new HashSet<T1>();
            // scan the list of elements to add once and only once
            foreach (var keyValuePair in elementsToAdd)
            {
                // in case of key found, the update only takes the old element
                dictionary.AddOrUpdate(keyValuePair.Key, keyValuePair.Value, (key, value) => value);

                // we report the element as present
                elementsPresent.Add(keyValuePair.Key);
            }

            // then we remove the elements not present:
            foreach (var res in dictionary.Where(o => !elementsPresent.Contains(o.Key)).ToList())
            {
                dictionary.TryRemove(res.Key, out _);
            }
        }
    }
}
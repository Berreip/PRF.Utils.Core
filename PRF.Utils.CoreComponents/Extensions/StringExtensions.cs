using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Les méthodes d'extensions lié au type 'string'
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Fait un replace sans tenir compte de la casse
        /// </summary>
        public static string ReplaceCaseInsensitive(this string str, string pattern, string replacedBy)
        {
            return Regex.Replace(str, pattern, replacedBy, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Returns the strintg between the first part and the second part AND NULL if a part is not found at all
        /// </summary>
        public static string GetBetween(this string s, string firstPart, string secondPart)
        {
            var from = s.LastIndexOf(firstPart, StringComparison.Ordinal) + firstPart.Length;
            var to = s.LastIndexOf(secondPart, StringComparison.Ordinal);
            return from < to
                ? s.Substring(from, to - from)
                : null;
        }

        /// <summary>
        /// Récupère un chemin relatif en supprimant la partie 'pathToRemove' mais en conservant les sous-dossiers qui suivent
        /// Marche avec ou sans le '\' à la fin de beforeRelativePath
        /// </summary>
        /// <param name="filefullName">le chemin complet du fichier fichier</param>
        /// <param name="pathToRemove">le chemin à supprimer</param>
        /// <returns>le chemin relatif restant</returns>
        public static string GetRelativePath(this string filefullName, string pathToRemove)
        {
            // efface le beforeRelativePath avec et sans '\'
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata ==> tati\file.txt (SANS '\')
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata\ ==> tati\file.txt (AVEC '\')
            return filefullName.Replace(pathToRemove + Path.DirectorySeparatorChar, string.Empty).Replace(pathToRemove, string.Empty);
        }

        /// <summary>
        /// Supprime les lignes vides (ou seulement occupé par des espaces)
        /// </summary>
        /// <param name="str">la string dont on souhaite supprimer les lignes vides</param>
        public static string RemoveEmptyLines(this string str)
        {
            return Regex.Replace(str, $@"^\s*[{Environment.NewLine}]*", string.Empty, RegexOptions.Multiline).TrimEnd();
        }

        /// <summary>
        /// Do a string Contains in a case insensitive way
        /// </summary>
        /// <param name="str">the string where we are looking for the searched input</param>
        /// <param name="searchedString">the searched string</param>
        public static bool ContainsInsensitive(this string str, string searchedString)
        {
            return str?.IndexOf(searchedString, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        /// <summary>
        /// Do a string Equals in a case insensitive way
        /// You could provide a specific Culture info. by default, CultureInfo.InvariantCulture is used
        /// </summary>
        /// <param name="str">the string where we are looking for the searched input</param>
        /// <param name="searchedString">the searched string</param>
        /// <param name="info">OPTIONAL: You could provide a specific Culture info. by default, CultureInfo.InvariantCulture is used</param>
        public static bool EqualsInsensitive(this string str, string searchedString, CultureInfo info = null)
        {
            return string.Compare(str, searchedString, info ?? CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0;
        }

        /// <summary>
        /// Do a string Equals in a case insensitive way and also ignore accentuated letter.
        /// You could provide a specific Culture info. by default, CultureInfo.InvariantCulture is used
        /// </summary>
        /// <param name="str">the string where we are looking for the searched input</param>
        /// <param name="searchedString">the searched string</param>
        /// <param name="info">OPTIONAL: You could provide a specific Culture info. by default, CultureInfo.InvariantCulture is used</param>
        public static bool EqualsInsensitiveAndIgnoreAccents(this string str, string searchedString, CultureInfo info = null)
        {
            return string.Compare(str, searchedString, info ?? CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0;
        }

        private static readonly string _illegal = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        /// <summary>
        /// Do a string StartsWith in a case insensitive way
        /// </summary>
        /// <param name="str">the string where we are looking for the searched input</param>
        /// <param name="searchedString">the searched string</param>
        public static bool StartsWithInsensitive(this string str, string searchedString)
        {
            return str.StartsWith(searchedString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Do a string EndsWith in a case insensitive way
        /// </summary>
        /// <param name="str">the string where we are looking for the searched input</param>
        /// <param name="searchedString">the searched string</param>
        public static bool EndsWithInsensitive(this string str, string searchedString)
        {
            return str.EndsWith(searchedString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Supprime les symboles incompatibles avec un nom de fichier
        /// </summary>
        /// <param name="str">la string dont on souhaite supprimer les symboles incompatibles avec un nom de fichier</param>
        public static string RemoveInvalidPathCharacters(this string str)
        {
            foreach (var c in _illegal)
            {
                str = str.Replace(c.ToString(), string.Empty);
            }
            return str;
        }
    }
}

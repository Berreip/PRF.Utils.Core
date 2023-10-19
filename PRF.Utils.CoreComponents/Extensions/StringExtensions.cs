using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension methods linked to the 'string' type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Do a replace without taking case into account
        /// </summary>
        public static string ReplaceCaseInsensitive(this string str, string pattern, string replacedBy)
        {
            return Regex.Replace(str, pattern, replacedBy, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Returns the string between the first part and the second part AND NULL if a part is not found at all
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
        /// Get a relative path by removing the 'pathToRemove' part but keeping the following subfolders
        /// Works with or without the '\' at the end of beforeRelativePath
        /// </summary>
        /// <param name="fileFullName">the full path of the file file</param>
        /// <param name="pathToRemove">the path to remove</param>
        /// <returns>the remaining relative path</returns>
        public static string GetRelativePath(this string fileFullName, string pathToRemove)
        {
            // clear the beforeRelativePath with and without '\'
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata ==> tati\file.txt (WITH '\')
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata\ ==> tati\file.txt (WITHOUT '\')
            return fileFullName.Replace(pathToRemove + Path.DirectorySeparatorChar, string.Empty).Replace(pathToRemove, string.Empty);
        }

        /// <summary>
        /// Remove empty lines (or only occupied by spaces)
        /// </summary>
        /// <param name="str">the string from which we wish to remove empty lines</param>
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
        /// Remove symbols incompatible with a file name
        /// </summary>
        /// <param name="str">the string from which we wish to remove symbols incompatible with a file name</param>
        public static string RemoveInvalidPathCharacters(this string str)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var c in _illegal)
            {
                str = str.Replace(c.ToString(), string.Empty);
            }

            return str;
        }
    }
}
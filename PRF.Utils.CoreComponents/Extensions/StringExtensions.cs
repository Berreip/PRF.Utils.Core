using System;
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
        /// Renvoie la string contenue entre la première partie et la seconde partie
        /// </summary>
        /// <param name="s">la string à examiner</param>
        /// <param name="firstPart">la première partie</param>
        /// <param name="secondPart">la seconde partie</param>
        /// <returns>la string contenue entre la première partie et la seconde partie</returns>
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

        private static readonly string _illegal = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

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

        /// <summary>
        /// Retourne la string entre double quote
        /// </summary>
        public static string ToQuotedPath(this string path)
        {
            return $"\"{path}\"";
        }
    }
}

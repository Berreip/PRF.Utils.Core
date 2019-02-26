using System;
using System.IO;
using System.Reflection;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Les méthodes d'extensions lié au 'Path'
    /// </summary>
    public static class PathExtension
    {
        /// <summary>
        /// La longueur maximal d'un path sur ce système
        /// </summary>
        public static int MaxPathLenght { get; } = SetMaxPathLenght();

        private const int DEFAULT_VALUE_UNC = 260; // valeur par défaut UNC
        private static int SetMaxPathLenght()
        {
            try
            {
                // reflection
                var maxPathField = typeof(Path).GetField("MaxPath",
                    BindingFlags.Static |
                    BindingFlags.GetField |
                    BindingFlags.NonPublic);

                return (int?)maxPathField?.GetValue(null) ?? DEFAULT_VALUE_UNC;
            }
            catch (Exception)
            {
                return DEFAULT_VALUE_UNC;
            }
        }
    }
}

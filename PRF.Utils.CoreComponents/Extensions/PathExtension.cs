using System;
using System.IO;
using System.Reflection;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension methods linked to the 'Path'
    /// </summary>
    public static class PathExtension
    {
        /// <summary>
        /// The maximum length of a path on this system
        /// </summary>
        public static int MaxPathLenght { get; } = SetMaxPathLenght();

        private const int DEFAULT_VALUE_UNC = 260; // default value UNC
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

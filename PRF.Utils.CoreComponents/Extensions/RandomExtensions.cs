using System;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension de la classe Random
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Renvoie true ou false au hasard
        /// </summary>
        public static bool NextBoolean(this Random rd)
        {
            return rd.NextDouble() >= 0.5;
        }

        /// <summary>
        /// Génère une valeur aléatoire entre -1 et 1
        /// </summary>
        public static double NextNumberBetweenOneAndLessOne(this Random rd)
        {
            return (rd.NextDouble() - 0.5) * 2;
        }
    }
}

using System;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Le module MathExt regroupe les méthodes statique lié à quelques fonctions mathématiques
    /// </summary>
    public static class MathExt
    {
        /// <summary>
        /// Donne le plus grand item entre deux valeurs implémentant IComparable
        /// </summary>
        /// <param name="item1">la première valeur</param>
        /// <param name="item2">la seconde valeur</param>
        /// <typeparam name="T">le type des deux valeurs</typeparam>
        /// <returns>la valeur la plus grande (défini par IComparable)</returns>
        /// <see cref="IComparable"/>
        public static T Max<T>(T item1, T item2) where T : IComparable
        {
            return item1.CompareTo(item2) > 0 ? item1 : item2;
        }

        /// <summary>
        /// Donne le plus petit item entre deux valeurs implémentant IComparable
        /// </summary>
        /// <param name="item1">la première valeur</param>
        /// <param name="item2">la seconde valeur</param>
        /// <typeparam name="T">le type des deux valeurs</typeparam>
        /// <returns>la valeur la plus petite (défini par IComparable)</returns>
        /// <see cref="IComparable"/>
        public static T Min<T>(T item1, T item2) where T : IComparable
        {
            return item1.CompareTo(item2) > 0 ? item2 : item1;
        }

        /// <summary>
        /// Implémentation typé pour les doubles: S'assure que le nombre demandé est situé entre min et max et limite sa valeur sinon
        /// </summary>
        /// <param name="val">la valeur</param>
        /// <param name="min">la valeur minimal admise (incluse) </param>
        /// <param name="max">la valeur maximal admise (incluse)</param>
        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            return val > max
                ? max
                : val;
        }

        /// <summary>
        /// Implémentation typé pour les int: S'assure que le nombre demandé est situé entre min et max et limite sa valeur sinon
        /// </summary>
        /// <param name="val">la valeur</param>
        /// <param name="min">la valeur minimal admise (incluse) </param>
        /// <param name="max">la valeur maximal admise (incluse)</param>
        public static int Clamp(int val, int min, int max)
        {
            if (val < min) return min;
            return val > max
                ? max
                : val;
        }

        /// <summary>
        /// S'assure que le type demandé est situé entre min et max et limite sa valeur sinon
        /// </summary>
        /// <param name="val">la valeur</param>
        /// <param name="min">la valeur minimal admise (incluse) </param>
        /// <param name="max">la valeur maximal admise (incluse)</param>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0) return min;

            return val.CompareTo(max) > 0
                ? max
                : val;
        }
    }
}

using System;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// The MathExt module groups together static methods linked to a few mathematical functions
    /// </summary>
    public static class MathExt
    {
        /// <summary>
        /// Returns the largest item between two values implementing IComparable
        /// </summary>
        /// <param name="item1">the first value</param>
        /// <param name="item2">the second value</param>
        /// <typeparam name="T">the type of the two values</typeparam>
        /// <returns>the largest value (defined by IComparable)</returns>
        /// <see cref="IComparable"/>
        public static T Max<T>(T item1, T item2) where T : IComparable
        {
            return item1.CompareTo(item2) > 0 ? item1 : item2;
        }

        /// <summary>
        /// Returns the smallest item between two values implementing IComparable
        /// </summary>
        /// <param name="item1">the first value</param>
        /// <param name="item2">the second value</param>
        /// <typeparam name="T">the type of the two values</typeparam>
        /// <returns>the smallest value (defined by IComparable)</returns>
        /// <see cref="IComparable"/>
        public static T Min<T>(T item1, T item2) where T : IComparable
        {
            return item1.CompareTo(item2) > 0 ? item2 : item1;
        }

        /// <summary>
        /// Typed implementation for doubles: Ensures that the requested number is between min and max and limits its value otherwise
        /// </summary>
        /// <param name="val">value</param>
        /// <param name="min">the minimum allowed value (inclusive) </param>
        /// <param name="max">the maximum allowed value (inclusive)</param>
        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            return val > max
                ? max
                : val;
        }

        /// <summary>
        /// Typed implementation for int: Ensures that the requested number is between min and max and limits its value otherwise
        /// </summary>
        /// <param name="val">value</param>
        /// <param name="min">the minimum allowed value (inclusive) </param>
        /// <param name="max">the maximum allowed value (inclusive)</param>
        public static int Clamp(int val, int min, int max)
        {
            if (val < min) return min;
            return val > max
                ? max
                : val;
        }

        /// <summary>
        /// Ensures that the requested type is between min and max and limits its value otherwise
        /// </summary>
        /// <param name="val">value</param>
        /// <param name="min">the minimum allowed value (inclusive) </param>
        /// <param name="max">the maximum allowed value (inclusive)</param>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0) return min;

            return val.CompareTo(max) > 0
                ? max
                : val;
        }
    }
}
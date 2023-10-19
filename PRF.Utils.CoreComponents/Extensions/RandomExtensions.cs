using System;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension of the Random class
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Return true or false randomly
        /// </summary>
        public static bool NextBoolean(this Random rd)
        {
            return rd.NextDouble() >= 0.5;
        }

        /// <summary>
        ///Generates a random value between -1 and 1
        /// </summary>
        public static double NextNumberBetweenOneAndLessOne(this Random rd)
        {
            return (rd.NextDouble() - 0.5) * 2;
        }
    }
}

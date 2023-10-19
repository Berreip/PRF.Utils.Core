namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension class for int types
    /// </summary>
    public static class IntExtension
    {
        /// <summary>
        /// Determines whether the value falls within a given range.
        /// </summary>
        /// <param name="i">Value</param>
        /// <param name="minValue">Minimum value (inclusive)</param>
        /// <param name="maxValue">Maximum value (inclusive)</param>
        public static bool IsInRange(this int i, int minValue, int maxValue)
        {
            return i >= minValue && i <= maxValue;
        }
    }
}
namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Classe d'extension pour les types int
    /// </summary>
    public static class IntExtension
    {
        /// <summary>
        /// Détermine si la valeur est comprise dans un intervalle donné.
        /// </summary>
        /// <param name="i">Valeur</param>
        /// <param name="minValue">Valeur minimale (incluse)</param>
        /// <param name="maxValue">Valeur maximale (incluse)</param>
        public static bool IsInRange(this int i, int minValue, int maxValue)
        {
            return i >= minValue && i <= maxValue;
        }
    }
}

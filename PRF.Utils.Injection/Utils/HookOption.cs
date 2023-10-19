namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// Les différents type d'interception disponible pour un interface
    /// </summary>
    public enum InterceptionHookOption
    {
        /// <summary>
        /// Intercepte toute les méthodes et les setter des propriétés d'un interface
        /// </summary>
        MethodsAndSetProperties,

        /// <summary>
        /// Intercepte les méthodes seulement mais pas les propriétés d'un interface
        /// </summary>
        MethodsOnly,

        /// <summary>
        /// Intercepte les méthodes et propriétés (Get ou Set)
        /// d'un interface ayant un InterceptionAttribute
        /// </summary>
        /// <seealso cref="InterceptionAttribute"/>
        /// <remarks>Attention, les attributs d'interceptions ne fonctionnent
        /// que sur les interfaces</remarks>
        InterceptionAttributOnly,
    }
}

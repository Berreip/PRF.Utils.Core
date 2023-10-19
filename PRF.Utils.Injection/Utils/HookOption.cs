namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// The different types of interception available for an interface
    /// </summary>
    public enum InterceptionHookOption
    {
        /// <summary>
        /// Intercepts all methods and setters of interface properties
        /// </summary>
        MethodsAndSetProperties,

        /// <summary>
        /// Intercepts methods only but not interface properties
        /// </summary>
        MethodsOnly,

        /// <summary>
        /// Intercepts methods and properties (Get or Set)
        /// of an interface having an InterceptionAttribute
        /// </summary>
        /// <seealso cref="InterceptionAttribute"/>
        /// <remarks>Be careful, the interception attributes do not work
        /// only on interfaces</remarks>
        InterceptionAttributeOnly,
    }
}
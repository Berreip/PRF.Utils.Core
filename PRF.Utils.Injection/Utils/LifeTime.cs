namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// The different types of possible lifetime for the registrations of a class in the injection container
    /// </summary>
    public enum LifeTime
    {
        /// <summary>
        /// The class will be created as a singleton and the same instance will be returned on each request by the container (most common behavior)
        /// </summary>
        Singleton,

        /// <summary>
        /// Scoped lifetime: the class will be reused within th current scope (may vary depending on the injection framework used)
        /// </summary>
        Scoped,

        /// <summary>
        /// the container will return a new instance of the class on each request made by the consumer
        /// </summary>
        Transient,
    }
}
namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// The list of all pre-configured interceptors for classic use. It is possible to define a custom interceptor by
    /// overriding the IInterceptor interface
    /// </summary>
    public enum PredefinedInterceptors
    {
        /// <summary>
        /// Interceptor that traces the start and end of each intercepted method and does a ToString on the parameters of this method
        /// </summary>
        MethodTraceInterceptor,

        /// <summary>
        /// Interceptor that tracks the time spent in each intercepted method
        /// </summary>
        TimeWatchInterceptor,
    }
}
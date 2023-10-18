using System;
using PRF.Utils.Injection.Interception.Interceptors;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.Injection.Interception.Helpers
{
    /// <summary>
    /// Handle interception in a more simple way
    /// </summary>
    internal static class InterceptionExtensions
    {
        public static Type GetMatchingInterceptor(this PredefinedInterceptors interceptorType)
        {
            switch (interceptorType)
            {
                case PredefinedInterceptors.MethodTraceInterceptor:
                    return typeof(MethodTraceInterceptor);
                case PredefinedInterceptors.TimeWatchInterceptor:
                    return typeof(TimeWatchInterceptor);
                default:
                    throw new ArgumentOutOfRangeException(nameof(interceptorType), interceptorType, null);
            }
        }
    }
}

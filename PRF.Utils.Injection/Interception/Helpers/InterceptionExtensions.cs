using System;
using PRF.Utils.Injection.Interception.Interceptors;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.Injection.Interception.Helpers
{
    /// <summary>
    /// Propose une façon d'intercepter des types de façon plus claire
    /// </summary>
    internal static class InterceptionExtensions
    {
        public static Type GetMatchingInterceptor(this PredefinedInterceptors interceptortype)
        {
            switch (interceptortype)
            {
                case PredefinedInterceptors.MethodTraceInterceptor:
                    return typeof(MethodTraceInterceptor);
                case PredefinedInterceptors.TimeWatchInterceptor:
                    return typeof(TimeWatchInterceptor);
                default:
                    throw new ArgumentOutOfRangeException(nameof(interceptortype), interceptortype, null);
            }
        }
    }
}

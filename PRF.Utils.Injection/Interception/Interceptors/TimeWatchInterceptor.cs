using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.Helpers;

namespace PRF.Utils.Injection.Interception.Interceptors
{
    /// <inheritdoc />
    /// <summary>
    /// Intercepteur qui trace le temps passé dans une méthode
    /// </summary>
    public sealed class TimeWatchInterceptor : IInterceptor
    {
        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            var declaringTypeName = invocation.Method.DeclaringType?.Name;

            // démarre un chrono
            var watch = Stopwatch.StartNew();
            try
            {
                // Appel de l'instance concrete.
                invocation.Proceed();
            }
            finally
            {
                // gestion des méthodes dont le type de retour est une Task (asynchrones)
                if (invocation.ReturnValue is Task task)
                {
                    invocation.ReturnValue = task.InterceptAsync(
                        invocation,
                        () => StopWatchAndTrace(invocation, declaringTypeName, watch),
                        result => StopWatchAndTrace(invocation, declaringTypeName, watch));
                }
                else
                {
                    StopWatchAndTrace(invocation, declaringTypeName, watch);
                }
               
            }
        }

        private static void StopWatchAndTrace(IInvocation invocation, string declaringTypeName, Stopwatch watch)
        {
            // stoppe le timer et trace le temps passé
            watch.Stop();
            // trace du temps passé
            Trace.TraceInformation($"TIME_{declaringTypeName}.{invocation.Method.Name} = {watch.ElapsedMilliseconds}ms");
        }
    }
}

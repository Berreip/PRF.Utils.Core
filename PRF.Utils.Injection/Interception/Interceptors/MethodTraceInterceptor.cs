using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.Helpers;

namespace PRF.Utils.Injection.Interception.Interceptors
{
    /// <inheritdoc />
    /// <summary>
    /// Intercepteur qui trace les entrées et les sorties d'une méthode
    /// Il est un peu plus compliqué car il intercepte également les méthodes asynchrones
    /// </summary>
    public sealed class MethodTraceInterceptor : IInterceptor
    {
        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            var declaringTypeName = invocation.Method.DeclaringType?.Name;

            // trace le début de l'appel
            Trace.TraceInformation($@"START_{declaringTypeName}.{invocation.Method.Name}({string.Join(", ", invocation.Arguments)})");
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
                        () => Trace.TraceInformation($"STOP_{declaringTypeName}.{invocation.Method.Name}"),
                        result => Trace.TraceInformation($"STOP_{declaringTypeName}.{invocation.Method.Name}=[{result}]"));
                }
                else
                {
                    Trace.TraceInformation(invocation.Method.ReturnType == typeof(void)
                        ? $"STOP_{declaringTypeName}.{invocation.Method.Name}"
                        : $"STOP_{declaringTypeName}.{invocation.Method.Name}=[{invocation.ReturnValue}]");
                }
            }
        }
    }
}
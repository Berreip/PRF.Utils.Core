using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.Helpers;

namespace PRF.Utils.Injection.Interception.Interceptors
{
    /// <inheritdoc />
    /// <summary>
    /// Interceptor that traces the inputs and outputs of a method
    /// It is a little more complicated because it also intercepts asynchronous methods
    /// </summary>
    public sealed class MethodTraceInterceptor : IInterceptor
    {
        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            var declaringTypeName = invocation.Method.DeclaringType?.Name;

            // traces the start of the call
            Trace.TraceInformation($"START_{declaringTypeName}.{invocation.Method.Name}({string.Join(", ", invocation.Arguments)})");
            try
            {
                // Calling the concrete instance.
                invocation.Proceed();
            }
            finally
            {
                // management of methods whose return type is a Task (asynchronous)
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
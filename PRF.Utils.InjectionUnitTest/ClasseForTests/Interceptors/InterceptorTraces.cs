using System.Diagnostics;
using Castle.DynamicProxy;

namespace PRF.Utils.InjectionUnitTest.ClasseForTests.Interceptors;

public class InterceptorTraceInjectTracer : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // trace le type de la cible et le type de l'intercepteur
        Trace.TraceInformation($"{invocation.Method.DeclaringType?.Name}_{GetType().Name}");
        invocation.Proceed();
    }
}

public class InterceptorTraceStatic : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // trace le type de la cible et le type de l'intercepteur
        Trace.TraceInformation($"{invocation.Method.DeclaringType?.Name}_{GetType().Name}");

        // Calls the decorated instance.
        invocation.Proceed();
    }
}
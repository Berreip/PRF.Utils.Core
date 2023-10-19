using System.Diagnostics;
using Castle.DynamicProxy;

// ReSharper disable ClassNeverInstantiated.Global

namespace PRF.Utils.InjectionUnitTest.ClassForTests.Interceptors;

public class InterceptorTraceInjectTracer : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // traces target type and interceptor type
        Trace.TraceInformation($"{invocation.Method.DeclaringType?.Name}_{GetType().Name}");
        invocation.Proceed();
    }
}

public class InterceptorTraceStatic : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // traces target type and interceptor type
        Trace.TraceInformation($"{invocation.Method.DeclaringType?.Name}_{GetType().Name}");

        // Calls the decorated instance.
        invocation.Proceed();
    }
}
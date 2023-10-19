using Castle.DynamicProxy;

namespace PRF.Utils.InjectionUnitTest.ClassForTests.Interceptors;

// ReSharper disable once ClassNeverInstantiated.Global
public class InterceptorDoNothing : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // juste un passe plat
        invocation.Proceed();
        //var decoratedType = invocation.InvocationTarget.GetType();
        //_logger.Log($"{decoratedType.Name} executed in {watch.ElapsedMilliseconds} ms.");
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class InterceptorDoNothing2 : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        // juste un passe plat
        invocation.Proceed();
        //var decoratedType = invocation.InvocationTarget.GetType();
        //_logger.Log($"{decoratedType.Name} executed in {watch.ElapsedMilliseconds} ms.");
    }
}
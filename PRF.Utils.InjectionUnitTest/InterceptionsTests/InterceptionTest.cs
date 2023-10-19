using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClassForTests;
using PRF.Utils.InjectionUnitTest.ClassForTests.Interceptors;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests;

[TestFixture]
public class InterceptionTest
{
    private IInjectionContainer _container;

    [SetUp]
    public void TestInitialize()
    {
        // mock:


        // instance de test:
        _container = new InjectionContainerSimpleInjector();
    }

    /// <summary>
    /// Case 1: test that the interception works
    /// </summary>
    [Test]
    public async Task InterceptionTestV1()
    {
        //Configuration
        var count = 0;
        var traceReceived = System.Array.Empty<TraceData>();

        using (var tracer = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer, PageSize = 2 }))
        {
            _container.RegisterInstance(tracer);

            _container.RegisterInterceptor<InterceptorTraceInjectTracer>(LifeTime.Singleton);
            _container.RegisterInterceptor<InterceptorTraceStatic>(LifeTime.Singleton);
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);

            _container.Intercept<IClassVoidTest>().With<InterceptorTraceInjectTracer>();
            _container.Intercept<IClassVoidTest>().With<InterceptorTraceStatic>();

            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            var instance = _container.Resolve<IClassVoidTest>();
            instance.MethodCall();

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return (with the two traces of the interceptors)
        Assert.AreEqual(1, count);
        Assert.IsNotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceInjectTracer"));
        Assert.IsNotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceStatic"));
    }

    [Test]
    public async Task InterceptionTestV2()
    {
        //Configuration
        var count = 0;
        var traceReceived = System.Array.Empty<TraceData>();

        using (var tracer = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer, PageSize = 2 }))
        {
            _container.RegisterInstance(tracer);

            _container.RegisterInterceptor<InterceptorTraceInjectTracer>(LifeTime.Singleton);
            _container.RegisterInterceptor<InterceptorTraceStatic>(LifeTime.Singleton);
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);

            _container.InterceptAny(type => type.Name.Contains("lassVoidT")).With<InterceptorTraceInjectTracer>();
            _container.InterceptAny(type => type.Name.Contains("lassVoidT")).With<InterceptorTraceStatic>();
                
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            var instance = _container.Resolve<IClassVoidTest>();
            instance.MethodCall();

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return (with the two traces of the interceptors)
        Assert.AreEqual(1, count);
        Assert.IsNotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceInjectTracer"));
        Assert.IsNotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceStatic"));
    }

    /// <summary>
    /// Test for properties == default plot
    /// </summary>
    [Test]
    public async Task InterceptionTest_Property()
    {
        //Configuration
        var count = 0;
        var traceReceived = System.Array.Empty<TraceData>();
        using (var tracer = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer, PageSize = 2 }))
        {
            _container.RegisterInstance(tracer);

            _container.RegisterInterceptor<InterceptorTraceInjectTracer>(LifeTime.Singleton);
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);

            _container.InterceptAny(type => type.Name.Contains("lassVoidT")).With<InterceptorTraceInjectTracer>();
                
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            var instance = _container.Resolve<IClassVoidTest>();
            instance.Prop = 45; // setter
            instance.Prop = 45; // setter x2
            _ = instance.Prop;

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return 
        Assert.AreEqual(1, count);
        Assert.AreEqual(2, traceReceived.Length); //*2 because two sets and the get is not traced
    }

    /// <summary>
    /// Test for == properties with no trace option
    /// </summary>
    [Test]
    public async Task InterceptionTest_Property_With_custom_Option()
    {
        //Configuration
        var count = 0;
        using (var tracer = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer, PageSize = 2 }))
        {
            _container.RegisterInstance(tracer);

            _container.RegisterInterceptor<InterceptorTraceInjectTracer>(LifeTime.Singleton);
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);

            _container.Intercept<IClassVoidTest>().With<InterceptorTraceInjectTracer>(InterceptionHookOption.MethodsOnly);
                
            tracer.OnTracesSent += _ =>
            {
                Interlocked.Increment(ref count);
            };

            //Test
            var instance = _container.Resolve<IClassVoidTest>();
            instance.Prop = 45; // setter
            _ = instance.Prop;

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // no return 
        Assert.AreEqual(0, count);
    }

    /// <summary>
    /// Test for attributes
    /// </summary>
    [Test]
    public async Task InterceptionTest_Attribute()
    {
        //Configuration
        int count;
        TraceData[] traceReceived;
        using (var tracer = new TraceSourceSync(
                   new TraceConfig
                   {
                       TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
                       PageSize = 10000,
                   }))
        {
            _container.RegisterInstance(tracer);

            _container.RegisterInterceptor<InterceptorTraceInjectTracer>(LifeTime.Singleton);
            _container.Register<IClassVoidAttributeTest, ClassVoidAttributeTest>(LifeTime.Singleton);

            _container.Intercept<IClassVoidAttributeTest>().With<InterceptorTraceInjectTracer>(InterceptionHookOption.InterceptionAttributeOnly);

            count = 0;
            traceReceived = null;
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            var instance = _container.Resolve<IClassVoidAttributeTest>();
            instance.Prop = 45; // setter
            _ = instance.Prop;
            instance.MethodCall();

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return 
        Assert.AreEqual(1, count);
        Assert.AreEqual(3, traceReceived.Length);
    }
}
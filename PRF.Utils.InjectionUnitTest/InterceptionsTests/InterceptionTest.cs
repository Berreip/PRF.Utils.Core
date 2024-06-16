using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClassForTests;
using PRF.Utils.InjectionUnitTest.ClassForTests.Interceptors;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests;


#pragma warning disable xUnit2002
[Collection("Trace Tests Collection No SYNC #1")] // indicate to xUnit that this collection should not be run in parallel (has been set on other file too)
public class InterceptionTest
{
    private readonly IInjectionContainer _container = new InjectionContainerSimpleInjector();

    /// <summary>
    /// Case 1: test that the interception works
    /// </summary>
    [Fact]
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
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.NotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceInjectTracer"));
        Assert.NotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceStatic"));
    }

    [Fact]
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
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.NotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceInjectTracer"));
        Assert.NotNull(traceReceived.Single(o => o.Message == "IClassVoidTest_InterceptorTraceStatic"));
    }

    /// <summary>
    /// Test for properties == default plot
    /// </summary>
    [Fact]
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
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return 
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length); //*2 because two sets and the get is not traced
    }

    /// <summary>
    /// Test for == properties with no trace option
    /// </summary>
    [Fact]
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
            await tracer.FlushAndCompleteAddingAsync();
        }

        // no return 
        Assert.Equal(0, count);
    }

    /// <summary>
    /// Test for attributes
    /// </summary>
    [Fact]
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
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return 
        Assert.Equal(1, count);
        Assert.Equal(3, traceReceived.Length);
    }
}
#pragma warning restore xUnit2002
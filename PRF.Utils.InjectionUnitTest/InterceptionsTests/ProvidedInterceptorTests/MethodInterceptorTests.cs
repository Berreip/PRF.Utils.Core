using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClassForTests;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests.ProvidedInterceptorTests;

[Collection("Trace Tests Collection No SYNC #1")] // indicate to xUnit that this collection should not be run in parallel (has been set on other file too)
public class MethodInterceptorTests
{
    private readonly IInjectionContainer _container;
    private readonly TraceConfig _traceConfig;

    public MethodInterceptorTests()
    {
        // set the current culture to En-Us for testing
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        // mock:
        _traceConfig = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            PageSize = 1_000_000,
            MaximumTimeForFlush = TimeSpan.FromSeconds(5),
        };

        // instance de test :
        _container = new InjectionContainerSimpleInjector();

        // saves the type of the test class
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
    }


    /// <summary>
    /// Case 1: test that the interception done via MethodTraceInterceptor works
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV1()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            instance.MethodCall();

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // a single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCall()", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCall", traceReceived[1].Message);
    }

    /// <summary>
    /// Case 1: test that the interception made via MethodTraceInterceptor works and is efficient
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestPerformanceV1()
    {
        //Configuration
        var count = 0;
        const int upper = 100_000;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();
        // delete all tracers (important in debug)
        _traceConfig.TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll;

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < upper; i++)
            {
                instance.MethodCall();
            }
            watch.Stop();

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1), $"TOO SLOW: The time spent is {watch.ElapsedMilliseconds} ms");
        }

        // a single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2 * upper, traceReceived.Length);
    }


    [Fact]
    public async Task MethodInterceptorTestV2()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);
        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            instance.MethodCallWithParameters("toto", 789, new CustomObject());

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWithParameters(toto, 789, surchargeCustomObject)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWithParameters", traceReceived[1].Message);
    }


    /// <summary>
    /// intercept test for properties (setter exclusively)
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV3()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);
        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            instance.Prop = 45;

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.set_Prop(45)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.set_Prop", traceReceived[1].Message);
    }


    /// <summary>
    /// intercept test for properties =&gt; check no trace getter
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV4()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);
        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = instance.Prop;

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(0, count);
        Assert.Empty(traceReceived);
    }

    [Fact]
    public async Task MethodInterceptorTestV5()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);
        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = instance.MethodCallWithParametersWithReturn("toto", 789, new CustomObject());

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWithParametersWithReturn(toto, 789, surchargeCustomObject)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWithParametersWithReturn=[569.489]", traceReceived[1].Message);
    }

    [Fact]
    public async Task MethodInterceptorTestV6()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);
        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = instance.TryGetValue(true, out _);

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.TryGetValue(True, 0)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.TryGetValue=[True]", traceReceived[1].Message);
    }

    [Fact]
    public async Task MethodInterceptorTestV7()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = instance.MethodCallWithParametersWithParams("param1", "param2");

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWithParametersWithParams(System.String[])", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWithParametersWithParams=[-1]", traceReceived[1].Message);
    }

    /// <summary>
    /// Handling asynchronous methods
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV8()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            await instance.MethodCallWaitAsync(TimeSpan.FromMilliseconds(50));

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWaitAsync(00:00:00.0500000)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWaitAsync", traceReceived[1].Message);
    }

    /// <summary>
    /// Handling asynchronous methods with return
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV9()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = await instance.MethodCallWaitWithReturnAsync(TimeSpan.FromMilliseconds(50));

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWaitWithReturnAsync(00:00:00.0500000)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWaitWithReturnAsync=[42]", traceReceived[1].Message);
    }

    /// <summary>
    /// Handling asynchronous methods with return
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTestV10()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            _ = await instance.MethodCallWaitWithReturnDataAsync(TimeSpan.FromMilliseconds(50));

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }
        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2, traceReceived.Length);
        Assert.Equal("START_IClassVoidTest.MethodCallWaitWithReturnDataAsync(00:00:00.0500000)", traceReceived[0].Message);
        Assert.Equal("STOP_IClassVoidTest.MethodCallWaitWithReturnDataAsync=[surchargeCustomObject]", traceReceived[1].Message);
    }

    /// <summary>
    /// Handling asynchronous methods with return
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTest_PerfV10()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();
        const int upper = 1_000;

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();
        _traceConfig.TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll;
        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            for (var i = 0; i < upper; i++)
            {
                _ = await instance.MethodCallForPerf();
            }

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }
        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2*upper, traceReceived.Length);
    }

    /// <summary>
    /// Handling asynchronous methods with return
    /// </summary>
    [Fact]
    public async Task MethodInterceptorTest_PerfV11()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();
        const int upper = 1_000;

        _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();
        _traceConfig.TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll;
        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            for (var i = 0; i < upper; i++)
            {
                _ = await instance.MethodCallForPerfInt();
            }

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }
        // A single return (with the two traces of the interceptors)
        Assert.Equal(1, count);
        Assert.Equal(2 * upper, traceReceived.Length);
    }
}
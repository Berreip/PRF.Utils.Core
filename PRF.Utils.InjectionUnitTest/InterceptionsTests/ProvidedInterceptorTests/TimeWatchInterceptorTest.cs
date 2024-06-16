using System;
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
public class TimeWatchInterceptorTest
{
    private readonly IInjectionContainer _container;
    private readonly TraceConfig _traceConfig;

    public TimeWatchInterceptorTest()
    {
        // mock:
        _traceConfig = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            PageSize = 1000,
            MaximumTimeForFlush = TimeSpan.FromSeconds(5),
        };

        // instance de test :
        _container = new InjectionContainerSimpleInjector();

        // saves the type of the test class
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
    }

    [Fact]
    public async Task TimeWatchInterceptorTestV1()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.TimeWatchInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.TimeWatchInterceptor);

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

        // A single return
        Assert.Equal(1, count);
        Assert.Single(traceReceived);
        Assert.StartsWith("TIME_IClassVoidTest.MethodCall = ", traceReceived[0].Message);
    }

    [Fact]
    public async Task TimeWatchInterceptorTestV2()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.TimeWatchInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.TimeWatchInterceptor);

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

        // A single return
        Assert.Equal(1, count);
        Assert.Single(traceReceived);
        Assert.StartsWith("TIME_IClassVoidTest.MethodCallWaitAsync = ", traceReceived[0].Message);
    }

    [Fact]
    public async Task TimeWatchInterceptorTestV3()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.TimeWatchInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.TimeWatchInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();

        using (var tracer = new TraceSourceSync(_traceConfig))
        {
            tracer.OnTracesSent += trace =>
            {
                traceReceived = trace;
                Interlocked.Increment(ref count);
            };

            //Test
            // ReSharper disable once MethodHasAsyncOverload
            instance.MethodCallWait(TimeSpan.FromMilliseconds(50));

            //Verify
            await tracer.FlushAndCompleteAddingAsync();
        }

        // A single return
        Assert.Equal(1, count);
        Assert.Single(traceReceived);
        Assert.StartsWith("TIME_IClassVoidTest.MethodCallWait = ", traceReceived[0].Message);
    }

    [Fact]
    public async Task TimeWatchInterceptorTestV4()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();

        _container.RegisterInterceptor(PredefinedInterceptors.TimeWatchInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.TimeWatchInterceptor);

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

        // A single return
        Assert.Equal(1, count);
        Assert.Single(traceReceived);
        Assert.StartsWith("TIME_IClassVoidTest.MethodCallWaitWithReturnDataAsync = ", traceReceived[0].Message);
    }

    [Fact]
    public async Task TimeWatchInterceptorTestV4Perf()
    {
        //Configuration
        var count = 0;
        var traceReceived = Array.Empty<TraceData>();
        const int upper = 1_000;

        _container.RegisterInterceptor(PredefinedInterceptors.TimeWatchInterceptor, LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.TimeWatchInterceptor);

        var instance = _container.Resolve<IClassVoidTest>();
        _traceConfig.TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll;
        _traceConfig.PageSize = upper;

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

        // A single return
        Assert.Equal(1, count);
        Assert.Equal(upper, traceReceived.Length);
    }
}
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClassForTests;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests.ProvidedInterceptorTests;

[TestFixture]
public class TimeWatchInterceptorTest
{
    private IInjectionContainer _container;
    private TraceConfig _traceConfig;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _traceConfig = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            PageSize = 1000,
            MaximumTimeForFlush = TimeSpan.FromSeconds(5),
        };

        // instance de test:
        _container = new InjectionContainerSimpleInjector();

        // saves the type of the test class
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
    }

    [Test]
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
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, traceReceived.Length);
        Assert.IsTrue(traceReceived[0].Message.StartsWith("TIME_IClassVoidTest.MethodCall = "));
    }

    [Test]
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
            await instance.MethodCallWaitAsync(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

            //Verify
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, traceReceived.Length);
        Assert.IsTrue(traceReceived[0].Message.StartsWith("TIME_IClassVoidTest.MethodCallWaitAsync = "));
    }

    [Test]
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
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, traceReceived.Length);
        Assert.IsTrue(traceReceived[0].Message.StartsWith("TIME_IClassVoidTest.MethodCallWait = "));
    }

    [Test]
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
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, traceReceived.Length);
        Assert.IsTrue(traceReceived[0].Message.StartsWith("TIME_IClassVoidTest.MethodCallWaitWithReturnDataAsync = "));
    }

    [Test]
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
            await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        // A single return
        Assert.AreEqual(1, count);
        Assert.AreEqual(upper, traceReceived.Length);
    }
}
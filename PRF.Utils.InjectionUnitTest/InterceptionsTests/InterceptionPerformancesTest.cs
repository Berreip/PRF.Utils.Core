using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClassForTests;
using PRF.Utils.InjectionUnitTest.ClassForTests.Interceptors;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests;


public class InterceptionPerformancesTest
{
    private readonly IInjectionContainer _container;

    public InterceptionPerformancesTest()
    {
        // mock:
        // set the current culture to En-Us for testing
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        // instance de test :
        _container = new InjectionContainerSimpleInjector();
    }

    /// <summary>
    /// test that creating a proxy does not cost too many resources
    /// </summary>
    [Fact]
    public void PerformanceNewClassTestV2()
    {
        //Configuration
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Transient);

        //Test
        const int upper = 10_000;
        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            _ = _container.Resolve<IClassVoidTest>();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// test that creating a proxy does not cost too many resources
    /// </summary>
    [Fact]
    public void PerformanceNewClassTest_TransientV3()
    {
        //Configuration
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        _container.RegisterInstance(new TraceSourceSync(config));

        _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Transient);
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Transient);
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();

        //Test
        const int upper = 10_000;
        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            _ = _container.Resolve<IClassVoidTest>();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// test that creating a proxy does not cost too many resources
    /// => In singleton, it is the mm instances that must return so we must be faster
    /// </summary>
    [Fact]
    public void PerformanceNewClassTest_singletonV3()
    {
        //Configuration
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        _container.RegisterInstance(new TraceSourceSync(config));

        _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Transient);
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();

        //Test
        const int upper = 10_000;
        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            _ = _container.Resolve<IClassVoidTest>();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1),
            $"Too slow to create {upper} transient object with singleton interceptor: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// test that creating a proxy does not cost too many resources
    /// </summary>
    [Fact]
    public void PerformanceNewClassTestV4()
    {
        //Configuration
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        _container.RegisterInstance(new TraceSourceSync(config));

        _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
        _container.RegisterInterceptor<InterceptorDoNothing2>(LifeTime.Transient); // records a second one to verify no disturbance
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing2>();

        //Test
        const int upper = 10_000;
        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            _ = _container.Resolve<IClassVoidTest>();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Overrides the save method to verify that the behavior is the same
    /// </summary>
    [Fact]
    public void PerformanceNewClassTestV5()
    {
        //Configuration
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        _container.RegisterInstance(new TraceSourceSync(config));

        _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
        _container.RegisterInterceptor<InterceptorDoNothing2>(LifeTime.Transient); // records a second one to verify no disturbance
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing2>();

        //Test
        const int upper = 10_000;
        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            _ = _container.Resolve<IClassVoidTest>();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// test that calling a method on a proxy does not cost too many resources
    /// </summary>
    [Fact(Skip = "performance test, do not use on server")]
    public void PerformanceCallTest2()
    {
        //Configuration
        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);

        //Test
        const int upper = 10_000_000;
        var r = _container.Resolve<IClassVoidTest>();

        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            r.MethodCall();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(500),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// test that calling a method on a proxy does not cost too many resources
    /// </summary>
    [Fact(Skip = "performance test, do not use on server")]
    public void PerformanceCallTest3()
    {
        //Configuration
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        _container.RegisterInstance(new TraceSourceSync(config));

        _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
        _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
        _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>(InterceptionHookOption.MethodsOnly);

        //Test
        const int upper = 1_000_000;
        var r = _container.Resolve<IClassVoidTest>();

        var watch = Stopwatch.StartNew();

        for (var i = 0; i < upper; i++)
        {
            r.MethodCall();
        }

        watch.Stop();

        //Verify
        Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(500),
            $"Too slow to create {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
    }
}
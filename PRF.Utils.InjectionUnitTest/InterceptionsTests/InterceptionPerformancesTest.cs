using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClasseForTests;
using PRF.Utils.InjectionUnitTest.ClasseForTests.Interceptors;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests
{
    [TestFixture]
    public class InterceptionPerformancesTest
    {
        private IInjectionContainer _container;

        [SetUp]
        public void TestInitialize()
        {
            // mock:
            // set the current culture to En-Us for testing
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // instance de test:
            _container = new InjectionContainerSimpleInjector();
        }

        /// <summary>
        /// test que la création d'un proxy ne coute pas trop de ressources
        /// </summary>
        [Test]
        public void PerformanceNewClassTestV2()
        {
            //Configuration
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Transient);

            //Test
            const int upper = 10_000;
            var watch = Stopwatch.StartNew();

            for (var i = 0; i < upper; i++)
            {
                var _ = _container.Resolve<IClassVoidTest>();
            }
            watch.Stop();

            //Verify
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1), 
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// test que la création d'un proxy ne coute pas trop de ressources
        /// </summary>
        [Test]
        public void PerformanceNewClassTest_TransientV3()
        {
            //Configuration
            var config = new TraceConfig {TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            _container.RegisterInstance(new TraceSourceSync(config));

            _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Transient);
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Transient);
            _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();

            //Test
            const int upper = 10_000;
            var watch = Stopwatch.StartNew();

            for (var i = 0; i < upper; i++)
            {
                var _ = _container.Resolve<IClassVoidTest>();
            }
            watch.Stop();

            //Verify
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1),
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// test que la création d'un proxy ne coute pas trop de ressources
        /// => En singleton, c'est les mm instances qui doivent revenir donc on doit être plus rapide
        /// </summary>
        [Test]
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
                var _ = _container.Resolve<IClassVoidTest>();
            }
            watch.Stop();

            //Verify
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1),
                $"Trop lent pour créer {upper} objet transient avec intercepteur en singleton: time = {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// test que la création d'un proxy ne coute pas trop de ressources
        /// </summary>
        [Test]
        public void PerformanceNewClassTestV4()
        {
            //Configuration
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            _container.RegisterInstance(new TraceSourceSync(config));

            _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
            _container.RegisterInterceptor<InterceptorDoNothing2>(LifeTime.Transient); // en enregistre un second pour vérifier la non perturbation
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
            _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();
            _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing2>();

            //Test
            const int upper = 10_000;
            var watch = Stopwatch.StartNew();

            for (var i = 0; i < upper; i++)
            {
                var _ = _container.Resolve<IClassVoidTest>();
            }
            watch.Stop();

            //Verify
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1),
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Remplace la méthode d'enregistrement pour vérifier que le comportement est le même
        /// </summary>
        [Test]
        public void PerformanceNewClassTestV5()
        {
            //Configuration
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            _container.RegisterInstance(new TraceSourceSync(config));

            _container.RegisterInterceptor<InterceptorDoNothing>(LifeTime.Singleton);
            _container.RegisterInterceptor<InterceptorDoNothing2>(LifeTime.Transient); // en enregistre un second pour vérifier la non perturbation
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
            _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing>();
            _container.Intercept<IClassVoidTest>().With<InterceptorDoNothing2>();

            //Test
            const int upper = 10_000;
            var watch = Stopwatch.StartNew();

            for (var i = 0; i < upper; i++)
            {
                var _ = _container.Resolve<IClassVoidTest>();
            }
            watch.Stop();

            //Verify
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1),
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// test que l'appel d'une méthode sur un proxy ne coute pas trop de ressources
        /// </summary>
        [Test]
        [Ignore("performance test, do not use on server")]
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
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromMilliseconds(500),
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }
        
        /// <summary>
        /// test que l'appel d'une méthode sur un proxy ne coute pas trop de ressources
        /// </summary>
        [Test]
        [Ignore("performance test, do not use on server")]
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
            Assert.IsTrue(watch.Elapsed < TimeSpan.FromMilliseconds(500),
                $"Trop lent pour créer {upper} objet transient: time = {watch.ElapsedMilliseconds} ms");
        }
    }
}

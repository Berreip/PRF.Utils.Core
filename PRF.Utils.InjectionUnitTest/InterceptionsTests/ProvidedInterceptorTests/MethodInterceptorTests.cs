using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Injection.Containers;
using PRF.Utils.Injection.Utils;
using PRF.Utils.InjectionUnitTest.ClasseForTests;
using PRF.Utils.Tracer;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.InjectionUnitTest.InterceptionsTests.ProvidedInterceptorTests
{
    [TestFixture]
    public class MethodInterceptorTests
    {
        private IInjectionContainer _container;
        private TraceConfig _traceConfig;

        [SetUp]
        public void TestInitialize()
        {
            // set the current culture to En-Us for testing
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // mock:
            _traceConfig = new TraceConfig
            {
                TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
                PageSize = 1_000_000,
                MaximumTimeForFlush = TimeSpan.FromSeconds(5)
            };

            // instance de test:
            _container = new InjectionContainerSimpleInjector();

            // enregistre le type de la classe de test
            _container.Register<IClassVoidTest, ClassVoidTest>(LifeTime.Singleton);
        }


        /// <summary>
        /// Cas 1: test que l'interception faite via MethodTraceInterceptor fonctionne
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV1()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCall()", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCall", traceReceived[1].Message);
        }

        /// <summary>
        /// Cas 1: test que l'interception faite via MethodTraceInterceptor fonctionne et est performante
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestPerformanceV1()
        {
            //Configuration
            var count = 0;
            const int upper = 100_000;
            TraceData[] traceReceived = { };

            _container.RegisterInterceptor(PredefinedInterceptors.MethodTraceInterceptor, LifeTime.Singleton);
            _container.Intercept<IClassVoidTest>().With(PredefinedInterceptors.MethodTraceInterceptor);

            var instance = _container.Resolve<IClassVoidTest>();
            // supprime tt les traceurs (influent en debug)
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
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
                Assert.IsTrue(watch.Elapsed < TimeSpan.FromSeconds(1), $"TROP LENT: Le temps passé est de {watch.ElapsedMilliseconds} ms");
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2 * upper, traceReceived.Length);
        }


        [Test]
        public async Task MethodInterceptorTestV2()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWithParameters(toto, 789, surchargeCustomObject)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWithParameters", traceReceived[1].Message);
        }


        /// <summary>
        /// test d'interception pour les propriétés (setter exclusivement)
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV3()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.set_Prop(45)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.set_Prop", traceReceived[1].Message);
        }


        /// <summary>
        /// test d'interception pour les propriétés => vérifie pas de trace getter
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV4()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = instance.Prop;

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, traceReceived.Length);
        }

        [Test]
        public async Task MethodInterceptorTestV5()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = instance.MethodCallWithParametersWithReturn("toto", 789, new CustomObject());

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWithParametersWithReturn(toto, 789, surchargeCustomObject)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWithParametersWithReturn=[569.489]", traceReceived[1].Message);
        }

        [Test]
        public async Task MethodInterceptorTestV6()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = instance.TryGetValue(true, out var _);

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.TryGetValue(True, 0)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.TryGetValue=[True]", traceReceived[1].Message);
        }

        [Test]
        public async Task MethodInterceptorTestV7()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = instance.MethodCallWithParametersWithParams("param1", "param2");

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWithParametersWithParams(System.String[])", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWithParametersWithParams=[-1]", traceReceived[1].Message);
        }

        /// <summary>
        /// Gestion des méthodes asynchrones
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV8()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWaitAsync(00:00:00.0500000)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWaitAsync", traceReceived[1].Message);
        }

        /// <summary>
        /// Gestion des méthodes asynchrones avec retour
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV9()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = await instance.MethodCallWaitWithReturnAsync(TimeSpan.FromMilliseconds(50));

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWaitWithReturnAsync(00:00:00.0500000)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWaitWithReturnAsync=[42]", traceReceived[1].Message);
        }

        /// <summary>
        /// Gestion des méthodes asynchrones avec retour
        /// </summary>
        [Test]
        public async Task MethodInterceptorTestV10()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };

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
                var _ = await instance.MethodCallWaitWithReturnDataAsync(TimeSpan.FromMilliseconds(50));

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.AreEqual("START_IClassVoidTest.MethodCallWaitWithReturnDataAsync(00:00:00.0500000)", traceReceived[0].Message);
            Assert.AreEqual("STOP_IClassVoidTest.MethodCallWaitWithReturnDataAsync=[surchargeCustomObject]", traceReceived[1].Message);
        }

        /// <summary>
        /// Gestion des méthodes asynchrones avec retour
        /// </summary>
        [Test]
        public async Task MethodInterceptorTest_PerfV10()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };
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
                    var _ = await instance.MethodCallForPerf();
                }

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2*upper, traceReceived.Length);
        }

        /// <summary>
        /// Gestion des méthodes asynchrones avec retour
        /// </summary>
        [Test]
        public async Task MethodInterceptorTest_PerfV11()
        {
            //Configuration
            var count = 0;
            TraceData[] traceReceived = { };
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
                    var _ = await instance.MethodCallForPerfInt();
                }

                //Verify
                await tracer.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            // un seul retour (avec les deux traces des intercepteurs)
            Assert.AreEqual(1, count);
            Assert.AreEqual(2 * upper, traceReceived.Length);
        }
    }
}

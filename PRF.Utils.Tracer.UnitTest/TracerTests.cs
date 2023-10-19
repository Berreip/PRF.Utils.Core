using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener;

namespace PRF.Utils.Tracer.UnitTest
{
    [TestFixture]
    public class TracerTests
    {
        [SetUp]
        public void TestInitialize()
        {
            foreach (TraceListener listener in Trace.Listeners)
            {
                Assert.AreNotEqual(listener.Name, "MainTracerSync", "one tracer remains in the list of static tracers = pollution");
            }
        }

        [Test]
        public async Task TraceListenerTestV1()
        {
            // setup
            int count;
            using (var traceListenerSync = new TraceListenerSync(TimeSpan.FromSeconds(1), 1000))
            {
                count = 0;
                traceListenerSync.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };

                try
                {
                    Trace.Listeners.Add(traceListenerSync);

                    //Test
                    Trace.TraceInformation("Method1");
                    await traceListenerSync.FlushAndCompleteAddingAsync().ConfigureAwait(false);
                }
                finally
                {

                    // retire le traceur
                    Trace.Listeners.Remove(traceListenerSync);
                }
            }

            //Verify
            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task TraceListenerTestV2()
        {
            // setup
            int count;
            using (var traceListenerSync = new TraceListenerSync(TimeSpan.FromSeconds(1), 1000))
            {
                count = 0;
                traceListenerSync.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };
                try
                {
                    Trace.Listeners.Add(traceListenerSync);

                    //Test
                    Trace.TraceInformation("Method1");
                    await traceListenerSync.FlushAndCompleteAddingAsync().ConfigureAwait(false);
                }
                finally
                {

                    // retire le traceur
                    Trace.Listeners.Remove(traceListenerSync);
                }
            }

            //Verify
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Vérifie que si on configure un tracer en DoNothing, on ne récupère pas les traces issues des traceurs statiques
        /// </summary>
        [Test]
        public async Task DoNothing()
        {
            // setup
            var count =0;
            using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.DoNothing }))
            {
                ts.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };

                //Test
                Trace.TraceInformation("Method1");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            //Verify
            Assert.AreEqual(0, count);
        }
        
        [Test]
        public async Task DefaultTraceLevelCheck()
        {
            // setup
            using (var ts = new TraceSourceSync())
            {
                //Verify
                Assert.AreEqual(SourceLevels.Information, ts.TraceLevel);
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Vérifie qu'onne pollue pas la liste des listeners
        /// </summary>
        [Test]
        public async Task CleanListenersTest()
        {
            // setup
            var listenerCount = Trace.Listeners.Count;
            
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAcces };
            using (var ts = new TraceSourceSync(config))
            {
               
                //Test
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            // vérification que l'on ne pollue pas les Listeners statiques:
            Assert.AreEqual(listenerCount, Trace.Listeners.Count);
            Assert.IsTrue(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != @"MainTracerSync"));
        }

        /// <summary>
        /// Vérifie qu'onne pollue pas la liste des listeners
        /// </summary>
        [Test]
        public async Task CleanListenersTestV2()
        {
            // setup
            var listenerCount = Trace.Listeners.Count;
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.DoNothing };
            using (var ts = new TraceSourceSync(config))
            {

                //Test
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            // vérification que l'on ne pollue pas les Listeners statiques:
            Assert.IsTrue(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != @"MainTracerSync"));
            Assert.AreEqual(listenerCount, Trace.Listeners.Count);
        }

        /// <summary>
        /// Vérifie qu'onne pollue pas la liste des listeners
        /// </summary>
        [Test]
        public async Task CleanListenersTestV3()
        {
            // setup
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {

                //Test
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            // vérification que l'on ne pollue pas les Listeners statiques MAIS que l'on a retiré le listener par défaut:
            Assert.IsTrue(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != "Default"));
            Assert.IsTrue(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != @"MainTracerSync"));
        }

        /// <summary>
        /// Vérifie qu'onne pollue pas la liste des listeners
        /// </summary>
        [Test]
        public async Task CleanListenersTestV4()
        {
            // setup
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll };
            using (var ts = new TraceSourceSync(config))
            {

                //Test
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            // vérification que la liste des Listeners statiques est vide:
            Assert.AreEqual(0, Trace.Listeners.Count);
        }

        [Test]
        public async Task TraceErrorTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived={};
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAcces };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                Trace.TraceError("TraceError");
                Trace.TraceError("format {0} - {1}", "param1", "param2");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.IsTrue(traceReceived.Contains("TraceError"));
            Assert.IsTrue(traceReceived.Contains(@"format {0} - {1}")); // pas de formatage par défaut => on laisse les arguments à coté
        }

        [Test]
        public async Task TraceInformationTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                Trace.TraceInformation("TraceInformation");
                Trace.TraceInformation("format TraceInformation {0} - {1}", "param1", "param2");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.IsTrue(traceReceived.Contains("TraceInformation"));
            Assert.IsTrue(traceReceived.Contains("format TraceInformation {0} - {1}"));
        }

        [Test]
        public async Task TraceWarningTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                Trace.TraceWarning("TraceWarning");
                Trace.TraceWarning("format TraceWarning {0} - {1}", "param1", "param2");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(2, traceReceived.Length);
            Assert.IsTrue(traceReceived.Contains("TraceWarning"));
            Assert.IsTrue(traceReceived.Contains("format TraceWarning {0} - {1}"));
        }

        [Test]
        public async Task WriteTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                Trace.WriteLine("WriteLine");
                Trace.Write("Write");
                Trace.Write(new object());
                Trace.Write(new object(), "Write+object");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(4, traceReceived.Length);
            Assert.IsTrue(traceReceived.Contains("WriteLine"));
            Assert.IsTrue(traceReceived.Contains("Write"));
            Assert.IsTrue(traceReceived.Contains("System.Object"));
            Assert.IsTrue(traceReceived.Contains("Write+object: System.Object"));
        }

        [Test]
        public async Task WriteIfTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                Trace.WriteIf(true, "WriteIf true");
                Trace.WriteIf(false, "WriteIf false");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(1, traceReceived.Length);
            Assert.IsTrue(traceReceived.Contains("WriteIf true"));
        }

        [Test]
        public async Task TraceDataTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                ts.TraceData(TraceEventType.Information, 32, "param1");
                ts.TraceData(TraceEventType.Information, 32, "param2", "param3");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(2, traceReceived.Length, $"array == {string.Join(", ", traceReceived)}");
            Assert.IsTrue(traceReceived.Contains("param1"), $"array == {string.Join(", ", traceReceived)}");
            Assert.IsTrue(traceReceived.Contains("param2, param3"), $"array == {string.Join(", ", traceReceived)}");
        }
        [Test]
        public async Task TraceDataTest_null()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                ts.TraceData(TraceEventType.Information, 32, null);
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }
            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(1, traceReceived.Length, $"array == {string.Join(", ", traceReceived)}");
            Assert.IsTrue(traceReceived.Contains("NULL_DATA"), $"array == {string.Join(", ", traceReceived)}");
        }

        [Test]
        public async Task TraceEventTest()
        {
            // setup
            var countCall = 0;
            string[] traceReceived = { };
            var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += o =>
                {
                    Interlocked.Increment(ref countCall);
                    // remplace sans lock car l'appel doit être unique et countCall sers de vérif
                    traceReceived = o.Select(t => t.Message).ToArray();
                };

                //Test
                ts.TraceEvent(TraceEventType.Information, 56);
                ts.TraceEvent(TraceEventType.Information, 56, "message");
                ts.TraceEvent(TraceEventType.Information, 56, "format {0} - {1}", "param1", "param2");
                await ts.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            //Verify
            Assert.AreEqual(1, countCall);
            Assert.AreEqual(3, traceReceived.Length);
            Assert.IsTrue(traceReceived.Contains("TraceEvent id:56"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
            Assert.IsTrue(traceReceived.Contains("message"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
            Assert.IsTrue(traceReceived.Contains("format {0} - {1}"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
        }
        
        [Test]
        public async Task Trace_Performance()
        {
            // setup
            const int upper = 100_000;
            var count = 0;
            var nbTraces = 0;
            var key = new object();

            var config = new TraceConfig
            {
                TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll,
                PageSize = upper,
                MaximumTimeForFlush = TimeSpan.FromSeconds(5)
            };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += t =>
                {
                    lock (key)
                    {
                        count++;
                        nbTraces += t.Length;
                    }
                };
                
                //Test
                for (var i = 0; i < upper; i++)
                {
                    Trace.TraceError("error test trace TU");
                }

                //Verify
                await ts.FlushAndCompleteAddingAsync();
            }

            // nombre de page renvoyé == 1 seule
            Assert.AreEqual(1, count);
            Assert.AreEqual(upper, nbTraces);
        }

        /// <summary>
        /// trace de performance utilisant un traceData plus simple (mais pas forcément plus rapide)
        /// </summary>
        [Test]
        public async Task Trace_Performance_Trace_Data()
        {
            // setup
            const int upper = 100_000;
            var count = 0;
            var nbTraces = 0;
            var key = new object();

            var config = new TraceConfig
            {
                TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll,
                PageSize = upper,
                MaximumTimeForFlush = TimeSpan.FromSeconds(5)
            };
            using (var ts = new TraceSourceSync(config))
            {
                ts.OnTracesSent += t =>
                {
                    lock (key)
                    {
                        count++;
                        nbTraces += t.Length;
                    }
                };

                var watch = Stopwatch.StartNew();
                ////Test
                for (var i = 0; i < upper; i++)
                {
                    Trace.Write("error test trace TU");
                }
                watch.Stop();

                //Verify
                await ts.FlushAndCompleteAddingAsync();
            }

            // number of pages returned == 1 only
            Assert.AreEqual(1, count);

            Assert.AreEqual(upper, nbTraces);
        }

        /// <summary>
        /// Test that for an empty page, we send nothing when we close the buffer
        /// </summary>
        [Test]
        public async Task FlushAndCompleteAddingAsync_EmptyPage_Test()
        {
            // setup
            var count = 0;
            using (var ts = new TraceSourceSync(new TraceConfig {TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
            {
                ts.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };

                //Verify
                await ts.FlushAndCompleteAddingAsync();
            }

            Assert.AreEqual(0, count); // pas de réception de page car page vide
        }
        
        /// <summary>
        /// Test that the trace does not pose a problem when the buffer has been closed
        /// </summary>
        [Test]
        public async Task TraceAfterCompleteAdding()
        {
            // setup
            var count = 0;
            using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
            {
                ts.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };

                //Verify
                await ts.FlushAndCompleteAddingAsync();
                Trace.TraceError("error test trace TU");
            }

            Assert.AreEqual(0, count); // pas de réception
        }

        /// <summary>
        /// Tests that the trace does not pose a problem when we have placed the traceSource
        /// </summary>
        [Test]
        public async Task TraceAfterDispose()
        {
            // setup
            var count = 0;
            using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
            {
                ts.OnTracesSent += _ =>
                {
                    Interlocked.Increment(ref count);
                };

                //Verify
                await ts.FlushAndCompleteAddingAsync();
            }

            Trace.TraceError("error test trace TU");

            Assert.AreEqual(0, count); // not received
        }
    }
}

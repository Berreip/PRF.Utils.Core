using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PRF.Utils.CoreComponents.Async.TaskPool;

// ReSharper disable MethodSupportsCancellation

// ReSharper disable ObjectCreationAsStatement

namespace PRF.Utils.CoreComponent.UnitTest.Async.TaskPool
{
    [TestFixture]
    internal sealed class PeformanceTaskPoolSizeTests
    {
        private int _poolMaximumSize = 10;
        private const int ITERATION = 200_000;

        public static readonly Dictionary<int, (int Iteration, long Total)> _means = new Dictionary<int, (int Iteration, long Total)>();

        [Test]
        [Repeat(10)]
        [Ignore("performance test")]
        public async Task Perfo_TaskPoolSizeCappedAlt()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(_poolMaximumSize);
            var tcs = new TaskCompletionSource<bool>();

            //Act
            var watch = Stopwatch.StartNew();
            int counter = 0;
            Parallel.For(0, ITERATION / 1000, i =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    sut.AddWork(ct =>
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            var g = i / (j + 1) + 20;
                        }

                        Interlocked.Increment(ref counter);
                    });
                }
            });

            sut.AddWork(ct => tcs.SetResult(true));
            await tcs.Task.ConfigureAwait(false);
            await sut.WaitIdleAsync();
            watch.Stop();

            //Assert
            Assert.AreEqual(ITERATION, counter);
            // add the new iteration
            var mean = AddMean(watch, 0);
            TestContext.WriteLine($"ellapsed = {watch.ElapsedMilliseconds} ms MEAN [{mean} ms]");
        }

        private static int AddMean(Stopwatch watch, int key)
        {
            if (_means.TryGetValue(key, out var tuple))
            {
                _means[key] = (tuple.Iteration +1, tuple.Total + watch.ElapsedMilliseconds);
            }
            else
            {
                _means.Add(key, (1, watch.ElapsedMilliseconds));
            }
            return (int)Math.Round(_means[key].Total / (double)_means[key].Iteration);
        }
    }
}
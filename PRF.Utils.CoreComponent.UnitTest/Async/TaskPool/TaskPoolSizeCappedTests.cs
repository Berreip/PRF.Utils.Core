using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ArbitraryLib;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Async.TaskPool;
using PRF.Utils.CoreComponents.Extensions;

// ReSharper disable MethodSupportsCancellation

// ReSharper disable ObjectCreationAsStatement

namespace PRF.Utils.CoreComponent.UnitTest.Async.TaskPool
{
    [TestFixture]
    internal sealed class TaskPoolSizeCappedTests
    {
        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-10)]
        [Timeout(100)]
        public void Create_TaskPoolSizeCapped_with_invalid_size_throws(int poolSize)
        {
            //Arrange

            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => new TaskPoolSizeCapped(poolSize));
        }

        [Test]
        [Repeat(1_000)]
        [Timeout(100)]
        public async Task AddWork_executes_the_task()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(ct => { Interlocked.Increment(ref counter); });
            await res.WaitAsync();

            //Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [Repeat(10)]
        [Timeout(1300)]
        public async Task AddWork_executes_the_async_task()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(async ct =>
            {
                // ReSharper disable once MethodSupportsCancellation
                await Task.Run(() => {});
                Interlocked.Increment(ref counter);
            });
            await res.WaitAsync();

            //Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [TestCase(2)]
        [TestCase(20)]
        [TestCase(200)]
        [TestCase(2000)]
        [TestCase(20000)]
        [Timeout(1000)]
        public async Task AddWork_multiple_times_executes_the_tasks(int number)
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(Environment.ProcessorCount);
            var counter = 0;
            var tcs = new TaskCompletionSource<bool>();

            //Act
            for (var i = 0; i < number; i++)
            {
                sut.AddWork(ct => { Interlocked.Increment(ref counter); });
            }

            sut.AddWork(ct => { tcs.SetResult(true); });
            await tcs.Task.ConfigureAwait(false);
            await sut.WaitAllAsync();

            //Assert
            Assert.AreEqual(number, counter);
        }

        [Test]
        [Timeout(1000)]
        public async Task AddWork_Then_wait_runner_to_ends_and_continue()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(2);
            var counter = 0;

            //Act
            for (var i = 0; i < 10; i++)
            {
                sut.AddWork(ct => Interlocked.Increment(ref counter));
            }

            await Task.Delay(50);

            var res = sut.AddWork(ct => Interlocked.Increment(ref counter));

            await res.WaitAsync();

            //Assert
            Assert.AreEqual(11, counter);
        }

        [Test]
        [Repeat(10)]
        [Timeout(3000)]
        public async Task AddWork_mix_sync_and_async_tasks()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(2);
            var counter = 0;

            //Act
            var resAsync = sut.AddWork(async ct =>
            {
                await Task.Run(() => {}, ct).ConfigureAwait(false);
                Interlocked.Increment(ref counter);
            });
            var res = sut.AddWork(ct => { Interlocked.Increment(ref counter); });
            await resAsync.WaitAsync();
            await res.WaitAsync();

            //Assert
            Assert.AreEqual(2, counter);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        [TestCase(100)]
        [Repeat(100)]
        [Timeout(1000)]
        public async Task TaskPoolSizeCapped_does_not_exceed_the_max_number_of_simultaneous_executions(int poolSize)
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(poolSize);
            var counter = 0;

            //Act
            Parallel.For(0, 1000, i =>
            {
                sut.AddWork(ct =>
                {
                    var newCount = Interlocked.Increment(ref counter);
                    if (newCount > poolSize)
                    {
                        Assert.Fail($"Max number of simultaneous executions was exceeded: real = {newCount} vs limit = {poolSize}");
                    }

                    Interlocked.Decrement(ref counter);
                });
            });
            await sut.WaitAllAsync();

            //Assert
        }

        [Test]
        [Repeat(1_000)]
        [Timeout(100)]
        public async Task Added_work_can_be_cancelled_when_already_started()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();
            var mrevFromTask = new ManualResetEventSlim();
            var ctStatus = false;

            //Act
            var res = sut.AddWork(ct =>
            {
                mrevFromTask.Set(); // unlock for cancellation
                // ReSharper disable once MethodSupportsCancellation
                mrev.Wait();
                ctStatus = ct.IsCancellationRequested;
            });
            mrevFromTask.Wait(); // wait for the task to really start
            //request the cancellation of the started task
            res.Cancel();
            //unlock the waiting task
            mrev.Set();

            await sut.WaitAllAsync();

            //Assert
            Assert.IsTrue(ctStatus);
        }

        [Test]
        [Repeat(1_000)]
        [Timeout(1000)]
        public async Task Added_work_can_be_executed_after_the_previous_work_throws()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrevFromTask = new ManualResetEventSlim();
            var counter = 0;

            //Act
            var previous = sut.AddWork(ct =>
            {
                mrevFromTask.Set(); // unlock for cancellation
                // ReSharper disable once MethodSupportsCancellation
                throw new InvalidOperationException("failed");
            });

            mrevFromTask.Wait(); // wait for the task to really start

            // await the taskWithException:
            Assert.ThrowsAsync<InvalidOperationException>(() => previous.WaitAsync());

            await sut
                .AddWork(ct => Interlocked.Increment(ref counter))
                .WaitAsync()
                .ConfigureAwait(false);

            //Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [Repeat(1_000)]
        [Timeout(100)]
        public async Task Added_work_can_be_cancelled_when_not_started_yet()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrevBlockingTask = new ManualResetEventSlim();
            var mrevAddNotStartedTask = new ManualResetEventSlim();
            var counter = 0;

            //Act

            //add work to block the pool
            sut.AddWork(ct =>
            {
                mrevAddNotStartedTask.Set(); // unlock adding new work when waiting task blocks the pool
                // ReSharper disable once MethodSupportsCancellation
                mrevBlockingTask.Wait();
            });
            //add work to the blocked pool
            mrevAddNotStartedTask.Wait(); //wait for pool to be blocked by waiting task
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });

            //request the cancellation of the not started task
            res.Cancel();
            //unlock the waiting task
            mrevBlockingTask.Set();

            await sut.WaitAllAsync();

            //Assert
            Assert.AreEqual(0, counter);
        }


        [Test]
        [Timeout(200)]
        public void CancelledTaskRaise_an_operation_cancelled_exception()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork((Action<CancellationToken>)(ct =>
            {
                mrev.Set();
                throw new OperationCanceledException("manual cancel");
            }));

            mrev.Wait();

            //Assert
            Assert.ThrowsAsync<TaskCanceledException>(() => res.WaitAsync());
        }

        [Test]
        public async Task AddWork_should_favor_parralelism_over_efficiency()
        {
            // when requesting 3 tasks on a taskPool of size 3, every one should be started in parralel,
            // we should not wait for short period of time to see if the tasks can be done by the same thread
            // (it is not the purpose of this coponent) 
            //Arrange
            var sut = new TaskPoolSizeCapped(3);
            var mrev = new ManualResetEventSlim();
            var counter = 0;

            //Act
            for (var i = 0; i < 3; i++)
            {
                sut.AddWork(ct =>
                {
                    Interlocked.Increment(ref counter);
                    mrev.Wait();
                });
            }

            var watch = Stopwatch.StartNew();
            while (counter != 3)
            {
                if (watch.Elapsed > TimeSpan.FromSeconds(2))
                {
                    Assert.Fail("timeout reached");
                }

                await Task.Delay(50).ConfigureAwait(false);
            }

            //Assert
            Assert.AreEqual(3, counter);
            // unlock and finish:
            mrev.Set();
            await sut.WaitAllAsync();
        }

        [Test]
        [Timeout(200)]
        public void CancelledTaskRaise_an_operation_cancelled_exception_for_async_work()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async ct =>
            {
                mrev.Set();
                await Task.CompletedTask.ConfigureAwait(false);
                throw new OperationCanceledException("manual cancel");
            });

            mrev.Wait();

            //Assert
            Assert.ThrowsAsync<TaskCanceledException>(() => res.WaitAsync());
        }

        [Test]
        [Timeout(200)]
        public void WaitAsync_get_the_exception_raised_during_work_execution()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork((Action<CancellationToken>)(ct =>
            {
                mrev.Set();
                throw new ArithmeticException("foo");
            }));

            mrev.Wait();

            //Assert
            Assert.ThrowsAsync<ArithmeticException>(() => res.WaitAsync());
        }

        [Test]
        [Timeout(200)]
        public void WaitAsync_get_the_exception_raised_during_work_async_execution()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async ct =>
            {
                mrev.Set();
                await Task.CompletedTask.ConfigureAwait(false);
                throw new ArithmeticException("foo");
            });

            mrev.Wait();

            //Assert
            Assert.ThrowsAsync<ArithmeticException>(() => res.WaitAsync());
        }

        [Test]
        public async Task WaitAsync_with_timeout_returns_false_when_timeout_reached()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(ct => mrev.Wait());

            var workEnded = await res.WaitAsync(TimeSpan.FromMilliseconds(100));
            mrev.Set();

            //Assert
            Assert.IsFalse(workEnded);
        }

        [Test]
        [Timeout(4000)]
        public async Task WaitAsync_with_timeout_returns_true_when_timeout_not_reached()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async ct =>
            {
                mrev.Set();
                await Task.Run(() => {}, ct);
            });
            mrev.Wait();

            var workEnded = await res.WaitAsync(TimeSpan.FromSeconds(3));

            //Assert
            Assert.IsTrue(workEnded);
        }

        [Test]
        [Timeout(1000)]
        public void Wait_sync_wait_blocking_until_end_of_work()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(ct => { Interlocked.Increment(ref counter); });
            res.Wait();

            //Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [Timeout(1000)]
        public void Wait_sync_waot_blocking_until_end_of_work_with_timeout()
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(ct => { Interlocked.Increment(ref counter); });
            res.Wait(TimeSpan.MaxValue);

            //Assert
            Assert.AreEqual(1, counter);
        }


        [Test]
        [Repeat(2_000)]
        public async Task Random_test_leads_to_expected_results()
        {
            //Arrange
            var rd = new Random();
            var poolSize = (int)Math.Round(rd.NextDouble() * 20 + 1);
            var parallelTasks = (int)Math.Round(rd.NextDouble() * 20 + 1);
            var syncTask = (int)Math.Round(rd.NextDouble() * 200 + 1);

            var sut = new TaskPoolSizeCapped(poolSize);

            //Act
            var counter = 0;
            Parallel.For(0, parallelTasks, i =>
            {
                for (var j = 0; j < syncTask; j++)
                {
                    if (rd.NextBoolean())
                    {
                        sut.AddWork(async ct =>
                        {
                            await Task.Run(() =>
                            {
                                for (var j = 0; j < 10; j++)
                                {
                                    var g = i / (j + 1) + 20;
                                }
                            }).ConfigureAwait(false);

                            Interlocked.Increment(ref counter);
                        });
                    }
                    else
                    {
                        sut.AddWork(ct =>
                        {
                            for (var k  = 0; k < 10; k++)
                            {
                                var g = i / (k + 1) + 20;
                            }

                            Interlocked.Increment(ref counter);
                        });
                    }
                }
            });

            await sut.WaitAllAsync().ConfigureAwait(false);

            //Assert
            var expectedTotal = parallelTasks*syncTask;
            // in case it output the wrong number, let it another chance
            var retry = 0;
            while(counter != expectedTotal && retry < 3)
            {
                TestContext.WriteLine($"retry = {retry}");
                await Task.Delay(10);
                await sut.WaitAllAsync().ConfigureAwait(false);
                retry++;
            }
            Assert.AreEqual(expectedTotal, counter, $"poolSize: {poolSize}, parallelTasks: {parallelTasks}, syncTask: {syncTask}");
        }
    }
}
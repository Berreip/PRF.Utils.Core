using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Async.TaskPool;
using PRF.Utils.CoreComponents.Extensions;
using Xunit.Abstractions;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ObjectCreationAsStatement

namespace PRF.Utils.CoreComponent.UnitTest.Async.TaskPool;

public sealed class TaskPoolSizeCappedTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TaskPoolSizeCappedTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_TaskPoolSizeCapped_with_invalid_size_throws(int poolSize)
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
        {
            //Arrange

            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => new TaskPoolSizeCapped(poolSize));
        });
    }

    [Theory]
    [Repeat(1_000)]
    public async Task AddWork_executes_the_task(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });
            await res.WaitAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(1, counter);
        }).ConfigureAwait(true);
    }

    [Theory]
    [Repeat(10)]
    public async Task AddWork_executes_the_async_task(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(async _ =>
            {
                // ReSharper disable once MethodSupportsCancellation
                await Task.Run(() => { }, CancellationToken.None).ConfigureAwait(false);
                Interlocked.Increment(ref counter);
            });
            await res.WaitAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(1, counter);
        }).ConfigureAwait(true);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    [InlineData(200)]
    [InlineData(2000)]
    [InlineData(20000)]
    public async Task AddWork_multiple_times_executes_the_tasks(int number)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(Environment.ProcessorCount);
            var counter = 0;
            var tcs = new TaskCompletionSource<bool>();

            //Act
            for (var i = 0; i < number; i++)
            {
                sut.AddWork(_ => { Interlocked.Increment(ref counter); });
            }

            sut.AddWork(_ => { tcs.SetResult(true); });
            await tcs.Task.ConfigureAwait(false);
            await sut.WaitIdleAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(number, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task AddWork_Then_wait_runner_to_ends_and_continue()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(2);
            var counter = 0;

            //Act
            for (var i = 0; i < 10; i++)
            {
                sut.AddWork(_ => Interlocked.Increment(ref counter));
            }

            await Task.Delay(50).ConfigureAwait(false);

            var res = sut.AddWork(_ => Interlocked.Increment(ref counter));

            await res.WaitAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(11, counter);
        }).ConfigureAwait(true);
    }

    [Theory]
    [Repeat(10)]
    public async Task AddWork_mix_sync_and_async_tasks(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(2);
            var counter = 0;

            //Act
            var resAsync = sut.AddWork(async ct =>
            {
                await Task.Run(() => { }, ct).ConfigureAwait(false);
                Interlocked.Increment(ref counter);
            });
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });
            await resAsync.WaitAsync().ConfigureAwait(false);
            await res.WaitAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(2, counter);
        }).ConfigureAwait(true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(100)]
    public async Task TaskPoolSizeCapped_does_not_exceed_the_max_number_of_simultaneous_executions(int poolSize)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(poolSize);
            var counter = 0;

            //Act
            Parallel.For(0, 1000, _ =>
            {
                sut.AddWork(_ =>
                {
                    var newCount = Interlocked.Increment(ref counter);
                    if (newCount > poolSize)
                    {
                        Assert.Fail($"Max number of simultaneous executions was exceeded: real = {newCount} vs limit = {poolSize}");
                    }

                    Interlocked.Decrement(ref counter);
                });
            });
            await sut.WaitIdleAsync().ConfigureAwait(false);

            //Assert
        }).ConfigureAwait(true);
    }

    [Theory]
    [Repeat(50)]
    public async Task Added_work_can_be_cancelled_when_already_started(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
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
                mrev.Wait(CancellationToken.None);
                ctStatus = ct.IsCancellationRequested;
            });
            mrevFromTask.Wait(); // wait for the task to really start
            //request the cancellation of the started task
            res.Cancel();
            //unlock the waiting task
            mrev.Set();

            await sut.WaitIdleAsync().ConfigureAwait(false);

            //Assert
            Assert.True(ctStatus);
        }).ConfigureAwait(true);
    }

    [Theory]
    [Repeat(50)]
    public async Task Added_work_can_be_executed_after_the_previous_work_throws(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrevFromTask = new ManualResetEventSlim();
            var counter = 0;

            //Act
            var previous = sut.AddWork(_ =>
            {
                mrevFromTask.Set(); // unlock for cancellation
                // ReSharper disable once MethodSupportsCancellation
                throw new InvalidOperationException("failed");
            });

            mrevFromTask.Wait(); // wait for the task to really start

            // await the taskWithException:
            await Assert.ThrowsAsync<InvalidOperationException>(() => previous.WaitAsync()).ConfigureAwait(false);

            await sut
                    .AddWork(_ => Interlocked.Increment(ref counter))
                    .WaitAsync().ConfigureAwait(false)
                ;

            //Assert
            Assert.Equal(1, counter);
        }).ConfigureAwait(true);
    }

    [Theory]
    [Repeat(50)]
    public async Task Added_work_can_be_cancelled_when_not_started_yet(int _)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrevBlockingTask = new ManualResetEventSlim();
            var mrevAddNotStartedTask = new ManualResetEventSlim();
            var counter = 0;

            //Act

            //add work to block the pool
            sut.AddWork(_ =>
            {
                mrevAddNotStartedTask.Set(); // unlock adding new work when waiting task blocks the pool
                // ReSharper disable once MethodSupportsCancellation
                mrevBlockingTask.Wait(CancellationToken.None);
            });
            //add work to the blocked pool
            mrevAddNotStartedTask.Wait(); //wait for pool to be blocked by waiting task
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });

            //request the cancellation of the not started task
            res.Cancel();
            //unlock the waiting task
            mrevBlockingTask.Set();

            await sut.WaitIdleAsync().ConfigureAwait(false);

            //Assert
            Assert.Equal(0, counter);
        }).ConfigureAwait(true);
    }


    [Fact]
    public async Task CancelledTaskRaise_an_operation_cancelled_exception()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork((Action<CancellationToken>)(_ =>
            {
                mrev.Set();
                throw new OperationCanceledException("manual cancel");
            }));

            mrev.Wait();

            //Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => res.WaitAsync()).ConfigureAwait(false);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task AddWork_should_favor_parallelism_over_efficiency()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            // when requesting 3 tasks on a taskPool of size 3, every one should be started in parallel,
            // we should not wait for short period of time to see if the tasks can be done by the same thread
            // (it is not the purpose of this component)
            //Arrange
            var sut = new TaskPoolSizeCapped(3);
            var mrev = new ManualResetEventSlim();
            var counter = 0;
            var pending = new List<IWorkInProgress>();

            //Act
            for (var i = 0; i < 3; i++)
            {
                pending.Add(sut.AddWork(_ =>
                {
                    Interlocked.Increment(ref counter);
                    mrev.Wait(CancellationToken.None);
                }));
            }

            var watch = Stopwatch.StartNew();
            while (counter != 3)
            {
                if (watch.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail($"timeout reached : {watch.ElapsedMilliseconds} ms");
                }

                await Task.Delay(50).ConfigureAwait(false);
            }

            //Assert
            Assert.Equal(3, counter);
            // unlock and finish:
            mrev.Set();
            await pending.WaitAllAsync().ConfigureAwait(false);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task CancelledTaskRaise_an_operation_cancelled_exception_for_async_work()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async _ =>
            {
                mrev.Set();
                await Task.CompletedTask.ConfigureAwait(false);
                throw new OperationCanceledException("manual cancel");
            });

            mrev.Wait();

            //Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => res.WaitAsync()).ConfigureAwait(false);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task WaitAsync_get_the_exception_raised_during_work_execution()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork((Action<CancellationToken>)(_ =>
            {
                mrev.Set();
                throw new ArithmeticException("foo");
            }));

            mrev.Wait();

            //Assert
            await Assert.ThrowsAsync<ArithmeticException>(() => res.WaitAsync()).ConfigureAwait(false);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task WaitAsync_get_the_exception_raised_during_work_async_execution()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async _ =>
            {
                mrev.Set();
                await Task.CompletedTask.ConfigureAwait(false);
                throw new ArithmeticException("foo");
            });

            mrev.Wait();

            //Assert
            await Assert.ThrowsAsync<ArithmeticException>(() => res.WaitAsync()).ConfigureAwait(false);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task WaitAsync_with_timeout_returns_false_when_timeout_reached()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(_ => mrev.Wait(CancellationToken.None));

            var workEnded = await res.WaitAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            mrev.Set();

            //Assert
            Assert.False(workEnded);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task WaitAsync_with_timeout_returns_true_when_timeout_not_reached()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var mrev = new ManualResetEventSlim();

            //Act
            var res = sut.AddWork(async ct =>
            {
                mrev.Set();
                await Task.Run(() => { }, ct).ConfigureAwait(false);
            });
            mrev.Wait();

            var workEnded = await res.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false);

            //Assert
            Assert.True(workEnded);
        }).ConfigureAwait(true);
    }

    [Fact]
    public void Wait_sync_wait_blocking_until_end_of_work()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });
            res.Wait();

            //Assert
            Assert.Equal(1, counter);
        });
    }

    [Fact]
    public void Wait_sync_not_blocking_until_end_of_work_with_timeout()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
        {
            //Arrange
            var sut = new TaskPoolSizeCapped(1);
            var counter = 0;

            //Act
            var res = sut.AddWork(_ => { Interlocked.Increment(ref counter); });
            res.Wait(TimeSpan.MaxValue);

            //Assert
            Assert.Equal(1, counter);
        });
    }


    [Fact]
    public async Task Random_test_leads_to_expected_results()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
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
                        sut.AddWork(async _ =>
                        {
                            await Task.Run(() =>
                            {
                                for (var k = 0; k < 10; k++)
                                {
                                    var __ = i / (k + 1) + 20;
                                }
                            }, CancellationToken.None).ConfigureAwait(false);

                            Interlocked.Increment(ref counter);
                        });
                    }
                    else
                    {
                        sut.AddWork(_ =>
                        {
                            for (var k = 0; k < 10; k++)
                            {
                                var __ = i / (k + 1) + 20;
                            }

                            Interlocked.Increment(ref counter);
                        });
                    }
                }
            });

            await sut.WaitIdleAsync().ConfigureAwait(false);

            //Assert
            var expectedTotal = parallelTasks * syncTask;
            // in case it output the wrong number, let it another chance
            var retry = 0;
            while (counter != expectedTotal && retry < 3)
            {
                _testOutputHelper.WriteLine($"retry = {retry}");
                await Task.Delay(10).ConfigureAwait(false);
                await sut.WaitIdleAsync().ConfigureAwait(false);
                retry++;
            }

            Assert.True(expectedTotal == counter, $"poolSize: {poolSize}, parallelTasks: {parallelTasks}, syncTask: {syncTask}");
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_async_callback_yield()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var sut = new TaskPoolSizeCapped(Environment.ProcessorCount);
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item()).ToArray();

            //Act
            await sut.ParallelForEachSizedCappedAsync(items: items, callbackAsync: async _ =>
            {
                await Task.Yield();
                Interlocked.Increment(ref counter);
            }).ConfigureAwait(false);

            //Assert
            Assert.Equal(1000, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_async_callback_with_real_wait()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var sut = new TaskPoolSizeCapped(200);
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item()).ToArray();

            //Act
            await sut.ParallelForEachSizedCappedAsync(items: items, callbackAsync: async _ =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                Interlocked.Increment(ref counter);
            }).ConfigureAwait(false);

            //Assert
            Assert.Equal(1000, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_sync_callback_yield()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var sut = new TaskPoolSizeCapped(Environment.ProcessorCount);
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item()).ToArray();

            //Act
            await sut.ParallelForEachSizedCappedAsync(items: items, callbackSync: _ => { Interlocked.Increment(ref counter); }).ConfigureAwait(false);

            //Assert
            Assert.Equal(1000, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_sync_callback_with_real_wait()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var sut = new TaskPoolSizeCapped(100);
            var counter = 0;
            var items = Enumerable.Range(0, 100).Select(_ => new Item()).ToArray();

            //Act
            await sut.ParallelForEachSizedCappedAsync(items: items, callbackSync: _ =>
            {
                Thread.Sleep(1);
                Interlocked.Increment(ref counter);
            }).ConfigureAwait(false);

            //Assert
            Assert.Equal(100, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_IEnumerable_Extensions_async_callback()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item());

            //Act
            await items.ParallelForEachSizedCappedAsync(poolMaximumSize: 10, callbackAsync: async _ =>
            {
                await Task.Yield();
                Interlocked.Increment(ref counter);
            }).ConfigureAwait(false);

            //Assert
            Assert.Equal(1000, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task ParallelForEachSizedCappedAsync_IEnumerable_Extensions_sync_callback()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item());

            //Act
            await items.ParallelForEachSizedCappedAsync(poolMaximumSize: 10, callbackSync: _ => { Interlocked.Increment(ref counter); }).ConfigureAwait(false);

            //Assert
            Assert.Equal(1000, counter);
        }).ConfigureAwait(true);
    }

    [Fact]
    public void ParallelForEachSizedCapped_IEnumerable_Extension()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
        {
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item());

            //Act
            items.ParallelForEachSizedCapped(poolMaximumSize: 10, _ => { Interlocked.Increment(ref counter); });

            //Assert
            Assert.Equal(1000, counter);
        });
    }

    [Fact]
    public void ParallelForEachSizedCapped_TaskPoolSizeCapped_Extension()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
        {
            var counter = 0;
            var items = Enumerable.Range(0, 1000).Select(_ => new Item());
            var sut = new TaskPoolSizeCapped(10);

            //Act
            sut.ParallelForEachSizedCapped(items: items, _ => { Interlocked.Increment(ref counter); });

            //Assert
            Assert.Equal(1000, counter);
        });
    }

    [Fact]
    public async Task Repro_scenario_ACH_long_running_first()
    {
        var sut = new TaskPoolSizeCapped(10);
        var mresFirstWaveEnd = new ManualResetEventSlim();
        var mresSecondWaveStart = new ManualResetEventSlim();
        const int neverEndingTasks = 4;
        var count = 0;

        for (var i = 0; i < neverEndingTasks; i++)
        {
            sut.AddWork(_ =>
            {
                Interlocked.Increment(ref count);
                mresFirstWaveEnd.Wait();
            });
        }

        while (count != neverEndingTasks)
        {
            await Task.Delay(10).ConfigureAwait(true);
        }

        //Act
        sut.AddWork(_ => { mresSecondWaveStart.Set(); });

        //Assert
        var res = mresSecondWaveStart.Wait(TimeSpan.FromSeconds(5));
        // free remaining
        mresFirstWaveEnd.Set();
        Assert.True(res);
    }

    [Fact]
    public async Task Repro_scenario_ACH_2_Long_running_but_less_than_works()
    {
        var sut = new TaskPoolSizeCapped(10);
        var mresFirstWaveEnd = new ManualResetEventSlim();
        var mresSecondWaveStart = new ManualResetEventSlim();
        const int neverEndingTasks = 4;
        var count = 0;

        for (var i = 0; i < neverEndingTasks; i++)
        {
            sut.AddWork(_ =>
            {
                Interlocked.Increment(ref count);
                mresFirstWaveEnd.Wait();
            });
        }

        while (count != neverEndingTasks)
        {
            await Task.Delay(10).ConfigureAwait(true);
        }

        //Act
        for (var i = 0; i < 6; i++)
        {
            sut.AddWork(async _ =>
            {
                mresSecondWaveStart.Set();
                await Task.CompletedTask.ConfigureAwait(false);
            });
        }

        //Assert
        var res = mresSecondWaveStart.Wait(TimeSpan.FromSeconds(5));
        // free remaining
        mresFirstWaveEnd.Set();

        Assert.True(res);
    }

    [Fact]
    public async Task AddWork_from_inside_a_running_work_is_executed()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            // Arrange
            var sut = new TaskPoolSizeCapped(2);
            var counter = 0;
            var added = new ManualResetEventSlim(false);

            // Act : le premier work enchaine un second work
            sut.AddWork(_ =>
            {
                sut.AddWork(_ => Interlocked.Increment(ref counter));
                added.Set(); // publication « second work ajouté »
            });

            added.Wait(); // la seconde tâche est en file
            await sut.WaitIdleAsync().ConfigureAwait(false); // laisse tout se vider

            // Assert
            Assert.Equal(1, counter);
        }).ConfigureAwait(true);
    }

    // -----------------------------------------------------------------------------
    //   Plusieurs appels concurrents à WaitIdleAsync
    // -----------------------------------------------------------------------------
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task WaitIdleAsync_can_be_awaited_concurrently(int waiters)
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            // Arrange
            var sut = new TaskPoolSizeCapped(4);
            var mrev = new ManualResetEventSlim(false);
            var tasks = new List<Task>();

            // « Bloque » un runner pour forcer un vrai travail en cours
            sut.AddWork(_ => mrev.Wait());

            // Lance N WaitIdleAsync en parallèle
            for (var i = 0; i < waiters; i++)
                tasks.Add(Task.Run(() => sut.WaitIdleAsync()));

            // Ajoute ensuite un travail
            sut.AddWork(_ => { });

            // Act : on libère le runner bloquant → tout doit finir
            mrev.Set();
            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Assert
            // Si on arrive ici, aucun deadlock ; rien de plus à vérifier
        }).ConfigureAwait(true);
    }

    [Fact]
    public async Task Test_Cancel_after_dispose_from_runner_using_block()
    {
        //Arrange
        var sut = new TaskPoolSizeCapped(1);
        var worksDisposed = new List<IWorkInProgress>();

        for (var i = 0; i < 5; i++)
        {
            var work = sut.AddWork(_ => { });
            var workAsync = sut.AddWork(async _ => { await Task.Yield(); });
            worksDisposed.Add(work);
            worksDisposed.Add(workAsync);
        }

        var cancellationBarrier = sut.AddWork(_ => { });
        await cancellationBarrier.WaitAsync().ConfigureAwait(true);

        //Act
        //Assert
        foreach (var workAlreadyDisposed in worksDisposed)
        {
            // Ensure the task is waitable even after being disposed
            await workAlreadyDisposed.WaitAsync().ConfigureAwait(true);

            // Ensure the task is cancellable even after being disposed
            workAlreadyDisposed.Cancel();
        }
    }

    /// <summary>
    /// Ensure the WaitAsync does not swallow the inner exception
    /// </summary>
    [Fact]
    public async Task WaitAsync_throw_with_inner_exception()
    {
        //Arrange
        var sut = new TaskPoolSizeCapped(1);
        var hasBeenRaised = false;
        var res = sut.AddWork(_ => throw new ArgumentException("specific exception"));

        //Act
        try
        {
            await res.WaitAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
        }
        catch (ArgumentException e)
        {
            Assert.Equal("specific exception", e.Message);
            hasBeenRaised = true;
        }

        //Assert
        Assert.True(hasBeenRaised);
    }

    private sealed class Item;
}
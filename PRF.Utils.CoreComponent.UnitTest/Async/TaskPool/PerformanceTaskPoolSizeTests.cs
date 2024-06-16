using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Async.TaskPool;
using Xunit.Abstractions;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ObjectCreationAsStatement
namespace PRF.Utils.CoreComponent.UnitTest.Async.TaskPool;

public sealed class PerformanceTaskPoolSizeTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const int POOL_MAXIMUM_SIZE = 10;
    private const int ITERATION = 200_000;

    private static readonly Dictionary<int, (int Iteration, long Total)> _means = new Dictionary<int, (int Iteration, long Total)>();

    public PerformanceTaskPoolSizeTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory(Skip = "performance test")]
    [Repeat(10)]
    public async Task Perfo_TaskPoolSizeCappedAlt(int _)
    {
        //Arrange
        var sut = new TaskPoolSizeCapped(POOL_MAXIMUM_SIZE);
        var tcs = new TaskCompletionSource<bool>();

        //Act
        var watch = Stopwatch.StartNew();
        var counter = 0;
        Parallel.For(0, ITERATION / 1000, i =>
        {
            for (var j = 0; j < 1000; j++)
            {
                sut.AddWork(_ =>
                {
                    for (var k = 0; k < 10; k++)
                    {
                        var unused = i / (k + 1) + 20;
                    }

                    Interlocked.Increment(ref counter);
                });
            }
        });

        sut.AddWork(_ => tcs.SetResult(true));
        await tcs.Task.ConfigureAwait(true);
        await sut.WaitIdleAsync().ConfigureAwait(true);
        watch.Stop();

        //Assert
        Assert.Equal(ITERATION, counter);
        // add the new iteration
        var mean = AddMean(watch, 0);
        _testOutputHelper.WriteLine($"elapsed = {watch.ElapsedMilliseconds} ms MEAN [{mean} ms]");
    }

    [Theory(Skip = "For Memory footprint")]
    [Repeat(10)]
    public async Task TaskPoolSizeCapped_Memory_FootPrint_Ensure_Ressource_are_disposed(int _)
    {
        var sut = new TaskPoolSizeCapped(20);
        var startTestTotalMemory = GC.GetTotalMemory(true);

        //Act
        for (var i = 0; i < 10_000_000; i++)
        {
            sut.AddWork(_ =>
            {
                // ReSharper disable once ConvertToConstant.Local
                var r = 56;
                // ReSharper disable once UnusedVariable : FOO METHOD
                var t = r * r;
            });
        }

        await sut.WaitIdleAsync().ConfigureAwait(true);

        //Assert
        var endTestTotalMemory = GC.GetTotalMemory(true);
        var sizeInBytes = endTestTotalMemory - startTestTotalMemory;
        _testOutputHelper.WriteLine($"Size in bytes = {sizeInBytes}");
    }

    private static int AddMean(Stopwatch watch, int key)
    {
        if (_means.TryGetValue(key, out var tuple))
        {
            _means[key] = (tuple.Iteration + 1, tuple.Total + watch.ElapsedMilliseconds);
        }
        else
        {
            _means.Add(key, (1, watch.ElapsedMilliseconds));
        }

        return (int)Math.Round(_means[key].Total / (double)_means[key].Iteration);
    }
}
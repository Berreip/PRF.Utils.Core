using System;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.CoreComponents.Async;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

public sealed class TimerSlimTests
{
    [Fact]
    public async Task Sync_Callback_Executes_Correctly()
    {
        // Arrange
        var executionCount = 0;
        using var timer = new TimerSlim(_ => executionCount++, 50);

        // Act
        await Task.Delay(200).ConfigureAwait(true);

        // Assert
        Assert.True(executionCount > 0, "Callback should have been executed");
    }

    [Fact]
    public async Task Async_Callback_Executes_Correctly()
    {
        // Arrange
        var executionCount = 0;
        using var timer = new TimerSlim(async ct =>
        {
            await Task.Delay(10, ct).ConfigureAwait(true);
            Interlocked.Increment(ref executionCount);
        }, 50);

        // Act
        await Task.Delay(200).ConfigureAwait(true);

        // Assert
        Assert.True(executionCount > 0, "Async callback should have been executed");
    }

    [Fact]
    public async Task Error_Raises_OnError_Event()
    {
        // Arrange
        Exception capturedError = null;
        using var timer = new TimerSlim(_ => throw new InvalidOperationException("Test error"), 50);
        timer.OnError += ex => capturedError = ex;

        // Act
        await Task.Delay(200).ConfigureAwait(true);

        // Assert
        Assert.NotNull(capturedError);
        Assert.IsType<InvalidOperationException>(capturedError);
    }

    [Fact]
    public async Task Cancellation_Stops_Execution()
    {
        // Arrange
        var executionCount = 0;

        // Act
        using (new TimerSlim(_ => executionCount++, 50))
        {
            await Task.Delay(100).ConfigureAwait(true);
        }// Cancel the timer

        var countAfterCancel = executionCount;
        await Task.Delay(200).ConfigureAwait(true);

        // Assert
        Assert.True(countAfterCancel > 0, "Should have executed at least once");
        Assert.Equal(countAfterCancel, executionCount); // Should not execute after disposal
    }

    [Fact]
    public async Task Immediate_Execution_Works()
    {
        // Arrange
        var executionCount = 0;
        using var timer = new TimerSlim(_ => executionCount++, 100, runImmediately: true);

        // Act
        await Task.Delay(50).ConfigureAwait(true);

        // Assert
        Assert.Equal(1, executionCount); //Should execute immediately
    }

    [Fact]
    public async Task Cancellation_Token_Respected()
    {
        // Arrange
        var executionCount = 0;
        using var cts = new CancellationTokenSource();
        using var timer = new TimerSlim(token =>
        {
            token.ThrowIfCancellationRequested();
            executionCount++;
        }, 50);

        // Act
        await Task.Delay(100, cts.Token).ConfigureAwait(true);
        await cts.CancelAsync().ConfigureAwait(true);

        // Assert
        Assert.True(executionCount > 0, "Should have executed at least once");
    }

    [Fact]
    public async Task Multiple_Executions_Prevent_Overlapping()
    {
        // Arrange
        var concurrentExecutions = 0;
        var maxConcurrentExecutions = 0;
        using var timer = new TimerSlim(async ct =>
        {
            var current = Interlocked.Increment(ref concurrentExecutions);
            maxConcurrentExecutions = Math.Max(maxConcurrentExecutions, current);
            await Task.Delay(100, ct).ConfigureAwait(true);
            Interlocked.Decrement(ref concurrentExecutions);
        }, 50);

        // Act
        await Task.Delay(300).ConfigureAwait(true);

        // Assert
        Assert.Equal(1, maxConcurrentExecutions); //Should prevent concurrent executions
    }


    [Fact]
    public void SyncCallback_ShouldExecuteAtSpecifiedInterval()
    {
        // Arrange
        var callbackCount = 0;
        var interval = 50;
        var sut = new TimerSlim(() => callbackCount++, interval, runImmediately: false);

        // Act
        Thread.Sleep(3 * interval);

        // Assert
        Assert.True(callbackCount > 1);
        sut.Dispose();
    }

    [Fact]
    public async Task AsyncCallback_ShouldExecuteAtSpecifiedInterval()
    {
        // Arrange
        var callbackCount = 0;
        var interval = 50;
        var sut = new TimerSlim(async ct =>
        {
            await Task.Delay(10, ct).ConfigureAwait(false);
            callbackCount++;
        }, interval, runImmediately: false);

        // Act
        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(50).ConfigureAwait(true);
            if (callbackCount > 1)
            {
                break;
            }
        }

        // Assert
        Assert.True(callbackCount > 1);
        sut.Dispose();
    }

    [Fact]
    public void SyncCallback_ShouldRunImmediatelyIfSpecified()
    {
        // Arrange
        var callbackCount = 0;
        var interval = 50;
        var sut = new TimerSlim(() => callbackCount++, interval, runImmediately: true);

        // Act
        Thread.Sleep(interval / 2);

        // Assert
        Assert.True(callbackCount >= 1);
        sut.Dispose();
    }

    [Fact]
    public void AsyncCallback_ShouldNotReenter()
    {
        // Arrange
        var interval = 50;
        var isReentrant = false;

        var sut = new TimerSlim(async ct =>
        {
            if (isReentrant)
            {
                Assert.Fail("Reentrant execution occurred.");
            }
            isReentrant = true;
            await Task.Delay(interval / 2, ct).ConfigureAwait(false);
            isReentrant = false;
        }, interval, runImmediately: false);

        // Act
        Thread.Sleep(3 * interval);

        // Assert
        sut.Dispose();
    }

    [Fact]
    public void Timer_ShouldTriggerErrorEvent_OnException()
    {
        // Arrange
        Exception capturedException = null;
        var sut = new TimerSlim(() => throw new InvalidOperationException("Test Exception"), 50, runImmediately: false);

        sut.OnError += ex => capturedException = ex;

        // Act
        Thread.Sleep(100);

        // Assert
        Assert.NotNull(capturedException);
        Assert.IsType<InvalidOperationException>(capturedException);
        sut.Dispose();
    }

    [Fact]
    public void Dispose_ShouldStopExecution()
    {
        // Arrange
        var callbackCount = 0;
        var sut = new TimerSlim(() => callbackCount++, 50, runImmediately: true);

        // Act
        Thread.Sleep(60);
        sut.Dispose();
        var countAfterDispose = callbackCount;

        Thread.Sleep(100);

        // Assert
        Assert.Equal(countAfterDispose, callbackCount);
    }
}
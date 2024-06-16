using Xunit.Sdk;

namespace CommonUnitTest;

public static class XunitTimeout
{
    /// <summary>
    /// Add a timeout in a unit test as xUnit does not have a [Timeout] attribute
    /// </summary>
    public static void Timeout(TimeSpan timeout, Action testMethod)
    {
        if (Task.WaitAny(Task.Run(testMethod), Task.Delay(timeout)) != 0)
        {
            throw new TestTimeoutException((int)timeout.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Add a timeout for an async unit test as xUnit does not have a [Timeout] attribute
    /// </summary>
    public static async Task TimeoutAsync(TimeSpan timeout, Func<Task> testMethodAsync)
    {
        var cts = new CancellationTokenSource();
        var delay = Task.Delay(timeout, cts.Token);
        if (await Task.WhenAny(testMethodAsync.Invoke(), delay) == delay)
        {
            throw new TestTimeoutException((int)timeout.TotalMilliseconds);
        }
        // cancel delay task and leave
        await cts.CancelAsync();
    }
}
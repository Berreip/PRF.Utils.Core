using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener;

namespace PRF.Utils.Tracer.UnitTest;

internal static class TraceTestHelper
{
    public static async Task<bool> AwaitTraceAsync(Action emitTrace, TraceConfig config = null, int timeoutMs = 500)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        config ??= new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.All,
        };

        using var ts = new TraceSourceSync(config);
        ts.OnTracesSent += _ => tcs.TrySetResult(true);

        emitTrace();

        await ts.FlushAndCompleteAddingAsync();
        return await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task;
    }

    public static async Task<IReadOnlyList<string>> CaptureTraceMessagesAsync(Action<TraceSourceSync> emitTraces, TraceConfig config = null)
    {
        var messages = new List<string>();
        config ??= new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.All,
        };

        using var ts = new TraceSourceSync(config);
        ts.OnTracesSent += traces =>
        {
            lock (messages)
            {
                messages.AddRange(traces.Select(t => t.Message));
            }
        };

        emitTraces(ts);
        await ts.FlushAndCompleteAddingAsync();
        return messages;
    }

    public static async Task WaitForStaticListenersToStabilize(Func<bool> predicate, int maxRetries = 20)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            if (predicate())
                return;

            await Task.Delay(100);
        }
    }

    public static async Task<bool> RunWithTemporaryListenerAsync(Func<TraceListenerSync, Task> emitAndFlushAsync, int timeoutMs = 500)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = new TraceListenerSync(TimeSpan.FromSeconds(1), 1000);
        listener.OnTracesSent += _ => tcs.TrySetResult(true);

        try
        {
            Trace.Listeners.Add(listener);
            await emitAndFlushAsync(listener);
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        return await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task;
    }
}
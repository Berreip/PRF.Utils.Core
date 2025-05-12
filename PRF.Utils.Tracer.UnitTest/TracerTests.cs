using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener;

namespace PRF.Utils.Tracer.UnitTest;

[Collection("Trace Tests Collection No SYNC #1")] // indicate to xUnit that this collection should not be run in parallel (has been set on other file too)
public class TracerTests
{
    public TracerTests()
    {
        foreach (TraceListener listener in Trace.Listeners)
        {
            Assert.True(listener.Name != "MainTracerSync", "one tracer remains in the list of static tracers = pollution");
        }
    }

    [Fact]
    public async Task TraceListenerTestV1()
    {
        var received = await TraceTestHelper.RunWithTemporaryListenerAsync(
            listener =>
            {
                Trace.TraceInformation("Method1");
                return listener.FlushAndCompleteAddingAsync();
            });

        Assert.True(received, "Expected one page of trace to be emitted and received");
    }

    [Fact]
    public async Task TraceListenerTestV2()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var listener = new TraceListenerSync(TimeSpan.FromSeconds(1), 1000);
        listener.OnTracesSent += _ => tcs.TrySetResult(true);

        try
        {
            Trace.Listeners.Add(listener);
            Trace.TraceInformation("Method1");
            await listener.FlushAndCompleteAddingAsync();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(500));
        Assert.True(completed == tcs.Task, "Expected trace to be received within 500ms");
    }

    /// <summary>
    /// Check that if you configure a tracer in DoNothing, you do not recover traces from static tracers
    /// </summary>
    [Fact]
    public async Task DoNothing()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.DoNothing });
        ts.OnTracesSent += _ => tcs.TrySetResult(true);

        Trace.TraceInformation("Method1");
        await ts.FlushAndCompleteAddingAsync();

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(300));
        Assert.False(completed == tcs.Task, "No static trace should be received in DoNothing mode");
    }

    [Fact]
    public async Task DefaultTraceLevelCheck()
    {
        // setup
        using (var ts = new TraceSourceSync())
        {
            //Verify
            Assert.Equal(SourceLevels.Information, ts.TraceLevel);
            await ts.FlushAndCompleteAddingAsync();
        }
    }

    /// <summary>
    /// verification that we do not pollute static Listeners:
    /// </summary>
    [Fact]
    public async Task CleanListenersTest()
    {
        // setup
        var listenerCount = Trace.Listeners.Count;

        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccess };
        using (var ts = new TraceSourceSync(config))
        {
            //Test
            await ts.FlushAndCompleteAddingAsync();
        }

        await TraceTestHelper.WaitForStaticListenersToStabilize(() => Trace.Listeners.Count == listenerCount);

        //Verify
        // verification that we do not pollute static Listeners:
        Assert.Equal(listenerCount, Trace.Listeners.Count);
        Assert.True(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != "MainTracerSync"));
    }

    /// <summary>
    /// verification that we do not pollute static Listeners:
    /// </summary>
    [Fact]
    public async Task CleanListenersTestV2()
    {
        // setup
        var listenerCount = Trace.Listeners.Count;
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.DoNothing };
        using (var ts = new TraceSourceSync(config))
        {
            //Test
            await ts.FlushAndCompleteAddingAsync();
        }

        // retry N times if needed
        await TraceTestHelper.WaitForStaticListenersToStabilize(() => Trace.Listeners.Count == listenerCount);

        //Verify
        // verification that we do not pollute static Listeners:
        Assert.Equal(listenerCount, Trace.Listeners.Count);
        Assert.True(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != "MainTracerSync"));
    }

    /// <summary>
    /// verification that we do not pollute static Listeners:
    /// </summary>
    [Fact]
    public async Task CleanListenersTestV3()
    {
        // setup
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            //Test
            await ts.FlushAndCompleteAddingAsync();
        }

        // retry N times if needed
        await TraceTestHelper.WaitForStaticListenersToStabilize(() => Trace.Listeners.Count == 0);

        //Verify
        // verification that we do not pollute the static Listeners BUT that we have removed the default listener:
        Assert.True(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != "Default"));
        Assert.True(Trace.Listeners.Cast<TraceListener>().All(o => o.Name != "MainTracerSync"));
    }

    /// <summary>
    /// verification that we do not pollute static Listeners:
    /// </summary>
    [Fact]
    public async Task CleanListenersTestV4()
    {
        // setup
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndClearAll };
        using (var ts = new TraceSourceSync(config))
        {
            //Test
            await ts.FlushAndCompleteAddingAsync();
        }

        var isEmpty = false;

        // retry N times if needed
        for (var i = 0; i < 20; i++)
        {
            isEmpty = Trace.Listeners.Count == 0;
            if (isEmpty)
            {
                break;
            }

            await Task.Delay(100);
        }

        //Verify
        // checking that the list of static Listeners is empty:
        Assert.True(isEmpty);
    }

    [Fact]
    public async Task TraceErrorTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccess };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            Trace.TraceError("TraceError");
            Trace.TraceError("format {0} - {1}", "param1", "param2");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Contains("TraceError", traceReceived);
        Assert.Contains("format {0} - {1}", traceReceived); // no formatting by default => we leave the arguments aside
    }

    [Fact]
    public async Task TraceInformationTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            Trace.TraceInformation("TraceInformation");
            Trace.TraceInformation("format TraceInformation {0} - {1}", "param1", "param2");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Equal(2, traceReceived.Length);
        Assert.Contains("TraceInformation", traceReceived);
        Assert.Contains("format TraceInformation {0} - {1}", traceReceived);
    }

    [Fact]
    public async Task TraceWarningTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            Trace.TraceWarning("TraceWarning");
            Trace.TraceWarning("format TraceWarning {0} - {1}", "param1", "param2");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Equal(2, traceReceived.Length);
        Assert.Contains("TraceWarning", traceReceived);
        Assert.Contains("format TraceWarning {0} - {1}", traceReceived);
    }

    [Fact]
    public async Task WriteTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            Trace.WriteLine("WriteLine");
            Trace.Write("Write");
            Trace.Write(new object());
            Trace.Write(new object(), "Write+object");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Equal(4, traceReceived.Length);
        Assert.Contains("WriteLine", traceReceived);
        Assert.Contains("Write", traceReceived);
        Assert.Contains("System.Object", traceReceived);
        Assert.Contains("Write+object: System.Object", traceReceived);
    }

    [Fact]
    public async Task WriteIfTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            Trace.WriteIf(true, "WriteIf true");
            Trace.WriteIf(false, "WriteIf false");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Single(traceReceived);
        Assert.Contains("WriteIf true", traceReceived);
    }

    [Fact]
    public async Task TraceDataTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            ts.TraceData(TraceEventType.Information, 32, "param1");
            ts.TraceData(TraceEventType.Information, 32, "param2", "param3");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.True(2 == traceReceived.Length, $"array == {string.Join(", ", traceReceived)}");
        Assert.True(traceReceived.Contains("param1"), $"array == {string.Join(", ", traceReceived)}");
        Assert.True(traceReceived.Contains("param2, param3"), $"array == {string.Join(", ", traceReceived)}");
    }

    [Fact]
    public async Task TraceDataTest_null()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            ts.TraceData(TraceEventType.Information, 32, null);
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.True(1 == traceReceived.Length, $"array == {string.Join(", ", traceReceived)}");
        Assert.True(traceReceived.Contains("NULL_DATA"), $"array == {string.Join(", ", traceReceived)}");
    }

    [Fact]
    public async Task TraceEventTest()
    {
        // setup
        var countCall = 0;
        var traceReceived = Array.Empty<string>();
        var config = new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer };
        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += o =>
            {
                Interlocked.Increment(ref countCall);
                // replaces without lock because the call must be unique and countCall serves as a check
                traceReceived = o.Select(t => t.Message).ToArray();
            };

            //Test
            ts.TraceEvent(TraceEventType.Information, 56);
            ts.TraceEvent(TraceEventType.Information, 56, "message");
            ts.TraceEvent(TraceEventType.Information, 56, "format {0} - {1}", "param1", "param2");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.Equal(1, countCall);
        Assert.Equal(3, traceReceived.Length);
        Assert.True(traceReceived.Contains("TraceEvent id:56"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
        Assert.True(traceReceived.Contains("message"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
        Assert.True(traceReceived.Contains("format {0} - {1}"), $"array contains: {string.Join(Environment.NewLine, traceReceived)}");
    }

    [Fact]
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
            MaximumTimeForFlush = TimeSpan.FromSeconds(5),
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

        // number of pages returned == 1 only
        Assert.Equal(1, count);
        Assert.Equal(upper, nbTraces);
    }

    /// <summary>
    /// performance trace using a simpler (but not necessarily faster) traceData
    /// </summary>
    [Fact]
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
            MaximumTimeForFlush = TimeSpan.FromSeconds(5),
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
        Assert.Equal(1, count);

        Assert.Equal(upper, nbTraces);
    }

    /// <summary>
    /// Test that for an empty page, we send nothing when we close the buffer
    /// </summary>
    [Fact]
    public async Task FlushAndCompleteAddingAsync_EmptyPage_Test()
    {
        // setup
        var count = 0;
        using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
        {
            ts.OnTracesSent += _ => { Interlocked.Increment(ref count); };

            //Verify
            await ts.FlushAndCompleteAddingAsync();
        }

        Assert.Equal(0, count); // pas de réception de page car page vide
    }

    /// <summary>
    /// Test that the trace does not pose a problem when the buffer has been closed
    /// </summary>
    [Fact]
    public async Task TraceAfterCompleteAdding()
    {
        // setup
        var count = 0;
        using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
        {
            ts.OnTracesSent += _ => { Interlocked.Increment(ref count); };

            //Verify
            await ts.FlushAndCompleteAddingAsync();
            Trace.TraceError("error test trace TU");
        }

        Assert.Equal(0, count); // pas de réception
    }

    /// <summary>
    /// Tests that the trace does not pose a problem when we have placed the traceSource
    /// </summary>
    [Fact]
    public async Task TraceAfterDispose()
    {
        // setup
        var count = 0;
        using (var ts = new TraceSourceSync(new TraceConfig { TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer }))
        {
            ts.OnTracesSent += _ => { Interlocked.Increment(ref count); };

            //Verify
            await ts.FlushAndCompleteAddingAsync();
        }

        Trace.TraceError("error test trace TU");

        Assert.Equal(0, count); // not received
    }
}
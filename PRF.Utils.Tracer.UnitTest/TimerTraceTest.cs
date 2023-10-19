using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.Tracer.UnitTest;

[TestFixture]
public class TimerTraceTest
{
    [SetUp]
    public void TestInitialize()
    {
        foreach (TraceListener listener in Trace.Listeners)
        {
            // static listener pollution check
            Assert.AreNotEqual(listener.Name, @"MainTracerSync", "one tracer remains in the list of static tracers = pollution");
        }
    }

    [Test]
    public async Task TimerTraceTestV1()
    {
        // setup
        var pagesReceived =0;
        // target date is now +0.5 seconds
        var timeTarget = DateTime.UtcNow.AddMilliseconds(500);

        // we decide to send a page every 100 ms, over half a second, we should have around 5 pages
        var config = new TraceConfig
        {
            PageSize = 1000,
            MaximumTimeForFlush = TimeSpan.FromMilliseconds(100),
        };

        using (var traceListener = new TraceSourceSync(config))
        {
            // when we receive a page, we increment the counter
            traceListener.OnTracesSent += _ => Interlocked.Increment(ref pagesReceived);

            // as long as the second is not reached, we trace messages (with a wait)
            while (DateTime.UtcNow < timeTarget)
            {
                Trace.TraceInformation("Method1");
                await Task.Delay(50);
            }

            //Test
            await traceListener.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        //Verify that we have 6 pages or less (the 5 flush + the possible last one)
        // Why less? => because depending on the Task.Delay, it is possible that no trace is written in a 200ms cycle and therefore nothing is sent
        Assert.IsTrue(pagesReceived is > 0 and <= 6, $"INVALID number of pages received = {pagesReceived}");
    }
}
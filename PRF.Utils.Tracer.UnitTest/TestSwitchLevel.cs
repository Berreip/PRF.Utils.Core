using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.Tracer.UnitTest;

[TestFixture]
public class TestSwitchLevel
{
    [SetUp]
    public void TestInitialize()
    {
        foreach (TraceListener listener in Trace.Listeners)
        {
            // static listener pollution check
            Assert.AreNotEqual(listener.Name, "MainTracerSync", "one tracer remains in the list of static tracers = pollution");
        }
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelInformationV1()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Information,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceInformation("test");
            await ts.FlushAndCompleteAddingAsync();
        }
            
        //Verify
        Assert.IsTrue(received); // SourceLevels.Information + Trace.TraceInformation = ok
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelInformationV2()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Information,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceWarning("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelInformationV3()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Information,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceError("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelInformationV4()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Information,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.Write("test", "category");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelErrorV1()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Error,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceInformation("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsFalse(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelErrorV2()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Error,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceWarning("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsFalse(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelErrorV3()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Error,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceError("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelErrorV4()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Error,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.Write("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelWarningV1()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Warning,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceInformation("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsFalse(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelWarningV2()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Warning,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceWarning("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelWarningV3()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Warning,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.TraceError("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Test]
    public async Task TestSwitchLevelWarningV4()
    {
        // setup
        var received = false;
        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Warning,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += _ => { received = true; };

            Trace.Write("test");
            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.IsTrue(received);
    }

    /// <summary>
    /// Test dynamic trace level change
    /// </summary>
    [Test]
    public async Task TestSwitchChangeLevelV1()
    {
        // setup
        var key = new object();
        var listMessages = new List<string>();

        var config = new TraceConfig
        {
            TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
            TraceLevel = SourceLevels.Error,
        };

        using (var ts = new TraceSourceSync(config))
        {
            ts.OnTracesSent += t =>
            {
                lock (key)
                {
                    listMessages.AddRange(t.Select(o => o.Message));
                }
            };

            Trace.TraceInformation("test1"); // pas tracé

            ts.TraceLevel = SourceLevels.Information;
            Trace.TraceInformation("test2"); // tracé

            ts.TraceLevel = SourceLevels.Warning;
            Trace.TraceInformation("test3"); // pas tracé

            await ts.FlushAndCompleteAddingAsync();
        }

        //Verify
        Assert.AreEqual(1, listMessages.Count);
        Assert.IsTrue(listMessages.Contains("test2"));
        Assert.IsFalse(listMessages.Contains("test1"));
        Assert.IsFalse(listMessages.Contains("test3"));
    }
}
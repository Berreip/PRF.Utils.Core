using System.Diagnostics;
using System.Threading.Tasks;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.Tracer.UnitTest;

public class TestSwitchLevel
{
    public TestSwitchLevel()
    {
        foreach (TraceListener listener in Trace.Listeners)
        {
            // static listener pollution check
            Assert.True(listener.Name != "MainTracerSync", "one tracer remains in the list of static tracers = pollution");
        }
    }

    private static TraceConfig Config(SourceLevels level) => new TraceConfig
    {
        TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
        TraceLevel = level,
    };

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelInformationV1()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceInformation("test"), Config(SourceLevels.Information));
        Assert.True(received); // SourceLevels.Information + Trace.TraceInformation = ok
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelInformationV2()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(
            () => Trace.TraceWarning("test"), Config(SourceLevels.Information));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelInformationV3()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(
            () => Trace.TraceError("test"), Config(SourceLevels.Information));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelInformationV4()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.Write("test", "category"), Config(SourceLevels.Information));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelErrorV1()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceInformation("test"), Config(SourceLevels.Error));
        Assert.False(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelErrorV2()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceWarning("test"), Config(SourceLevels.Error));
        Assert.False(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelErrorV3()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceError("test"), Config(SourceLevels.Error));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelErrorV4()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.Write("test"), Config(SourceLevels.Error));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelWarningV1()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceInformation("test"), Config(SourceLevels.Warning));
        Assert.False(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelWarningV2()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceWarning("test"), Config(SourceLevels.Warning));
        Assert.True(received);
    }

    /// <summary>
    /// Test trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelWarningV3()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.TraceError("test"), Config(SourceLevels.Warning));
        Assert.True(received);
    }

    /// <summary>
    /// TTest trace level
    /// </summary>
    [Fact]
    public async Task TestSwitchLevelWarningV4()
    {
        var received = await TraceTestHelper.AwaitTraceAsync(() => Trace.Write("test"), Config(SourceLevels.Warning));
        Assert.True(received);
    }

    /// <summary>
    /// Test dynamic trace level change
    /// </summary>
    [Fact]
    public async Task TestSwitchChangeLevelV1()
    {
        // setup
        var messages = await TraceTestHelper.CaptureTraceMessagesAsync(ts =>
            {
                Trace.TraceInformation("test1"); // ignoré

                ts.TraceLevel = SourceLevels.Information;
                Trace.TraceInformation("test2"); // capturé

                ts.TraceLevel = SourceLevels.Warning;
                Trace.TraceInformation("test3"); // ignoré
            },
            new TraceConfig
            {
                TraceBehavior = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer,
                TraceLevel = SourceLevels.Error,
            });

        Assert.Single(messages);
        Assert.Contains("test2", messages);
        Assert.DoesNotContain("test1", messages);
        Assert.DoesNotContain("test3", messages);
    }
}
using System;
using PRF.Utils.Tracer.Listener;

namespace PRF.Utils.Tracer.UnitTest;

[Collection("Trace Tests Collection No SYNC #1")] // indicate to xUnit that this collection should not be run in parallel (has been set on other file too)
public class TraceListenerTest
{
    /// <summary>
    /// Tests that the flush time must be greater than or equal to 50 ms
    /// </summary>
    [Fact]
    public void CtorV1()
    {
        //Configuration

        //Test

        Assert.Throws<ArgumentException>(() =>
        {
            using (var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(49), 1_000)) 
            { 
            }
        });
        //Verify
    }


    /// <summary>
    /// Tests that the buffer cannot be negative or equal to zero
    /// </summary>
    [Fact]
    public void CtorV2()
    {
        //Configuration

        //Test
        Assert.Throws<ArgumentException>(() =>
        {
            using var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(490), -1);
        });

        //Verify
    }


    /// <summary>
    /// Test that dispose does not trigger any problems
    /// </summary>
    [Fact]
    public void CtorV3()
    {
        //Configuration

        //Test
        using (var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(490), 1_000))
        {
        }

        //Verify
    }



    /// <summary>
    /// Tests the listener time limit (between a minimum and a maximum)
    /// </summary>
    [Fact]
    public void CtorV4()
    {
        //Configuration

        //Test           
        Assert.Throws<ArgumentException>(() =>
        {
            using var _ = new TraceListenerSync(TimeSpan.FromHours(2), 1_000);
        });
            
        //Verify
    }
}
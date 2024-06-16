using System.Threading;
using PRF.Utils.CoreComponents.Diagnostic;

namespace PRF.Utils.CoreComponent.UnitTest.Diagnostic;

public sealed class DebugCoreTests
{
    [Fact]
    public void Debug_Fail_does_not_throw_when_ignore_callback_has_been_registered()
    {
        //Arrange
        DebugCore.SetAssertionFailedCallback(_ => AssertionResponse.Ignore);

        //Act
        DebugCore.Fail("message");

        //Assert
    }
        
    [Fact]
    public void Debug_Fail_does_not_throw_when_no_callback_has_been_registered()
    {
        //Arrange

        //Act
        DebugCore.Fail("message");

        //Assert
    }

    [Fact]
    public void Debug_Assert_false_does_not_throw_when_ignore_callback_has_been_registered()
    {
        //Arrange
        DebugCore.SetAssertionFailedCallback(_ => AssertionResponse.Ignore);

        //Act
        DebugCore.Assert(false, "message");

        //Assert
    }

    [Fact]
    public void Debug_Assert_true_does_not_calls_registered_callback()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(_ =>
        {
            Interlocked.Increment(ref count);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Assert(true, "message");

        //Assert
        Assert.Equal(0, count);
    }
        
    [Fact]
    public void Debug_Fail_provide_expected_message()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.Equal("message", res.Message);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.Equal(1, count);
#else
            Assert.Equal(0, count);
#endif
    }
        
    [Fact]
    public void Debug_Fail_provide_expected_SourceMethod()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.Equal(nameof(Debug_Fail_provide_expected_SourceMethod), res.SourceMethod);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.Equal(1, count);
#else
            Assert.Equal(0, count);
#endif
    }
        
    [Fact]
    public void Debug_Fail_provide_expected_stackTrace()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.NotEmpty(res.StackTrace);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.Equal(1, count);
#else
            Assert.Equal(0, count);
#endif
    }
}
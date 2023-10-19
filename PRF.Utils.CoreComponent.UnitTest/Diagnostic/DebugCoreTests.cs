using System.Threading;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Diagnostic;

namespace PRF.Utils.CoreComponent.UnitTest.Diagnostic;

[TestFixture]
internal sealed class DebugCoreTests
{
    [Test]
    public void Debug_Fail_does_not_throw_when_ignore_callback_has_been_registered()
    {
        //Arrange
        DebugCore.SetAssertionFailedCallback(_ => AssertionResponse.Ignore);

        //Act
        DebugCore.Fail("message");

        //Assert
        Assert.Pass();
    }
        
    [Test]
    public void Debug_Fail_does_not_throw_when_no_callback_has_been_registered()
    {
        //Arrange

        //Act
        DebugCore.Fail("message");

        //Assert
        Assert.Pass();
    }

    [Test]
    public void Debug_Assert_false_does_not_throw_when_ignore_callback_has_been_registered()
    {
        //Arrange
        DebugCore.SetAssertionFailedCallback(_ => AssertionResponse.Ignore);

        //Act
        DebugCore.Assert(false, "message");

        //Assert
        Assert.Pass();
    }

    [Test]
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
        Assert.AreEqual(0, count);
    }
        
    [Test]
    public void Debug_Fail_provide_expected_message()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.AreEqual("message", res.Message);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.AreEqual(1, count);
#else
            Assert.AreEqual(0, count);
#endif
    }
        
    [Test]
    public void Debug_Fail_provide_expected_SourceMethod()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.AreEqual(nameof(Debug_Fail_provide_expected_SourceMethod), res.SourceMethod);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.AreEqual(1, count);
#else
            Assert.AreEqual(0, count);
#endif
    }
        
    [Test]
    public void Debug_Fail_provide_expected_stackTrace()
    {
        //Arrange
        var count = 0;
        DebugCore.SetAssertionFailedCallback(res =>
        {
                
            Interlocked.Increment(ref count);
            Assert.IsNotEmpty(res.StackTrace);
            return AssertionResponse.Ignore;
        });

        //Act
        DebugCore.Fail("message");

        //Assert
#if DEBUG
        Assert.AreEqual(1, count);
#else
            Assert.AreEqual(0, count);
#endif
    }
}
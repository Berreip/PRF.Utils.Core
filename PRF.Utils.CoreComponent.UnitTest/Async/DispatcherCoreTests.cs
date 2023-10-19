using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponent.UnitTest.Async;

[TestFixture]
internal sealed class DispatcherCoreTests
{
    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal()
    {
        // Arrange
        var count = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () => Interlocked.Increment(ref count),
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_with_exception_And_No_OnError_Callback()
    {
        // Arrange
        var count = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () =>
            {
                Interlocked.Increment(ref count);
                throw new Exception();
            },
            null).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_with_exception_on_finally()
    {
        // Arrange
        var count = 0;
        var countFinally = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () => Interlocked.Increment(ref count),
            null,
            () =>
            {
                Interlocked.Increment(ref countFinally);
                throw new Exception();
            }).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, countFinally);
    }


    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_Exception()
    {
        // Arrange
        var count = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () =>
            {
                Interlocked.Increment(ref count);
                throw new Exception();
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_Finally()
    {
        // Arrange
        var mainCalls = 0;
        var finallyCalls = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () =>
            {
                Interlocked.Increment(ref mainCalls);
            },
            _ => Interlocked.Increment(ref exception), // On exception
            () => Interlocked.Increment(ref finallyCalls)).ConfigureAwait(false); // On finally

        // Assert
        Assert.AreEqual(1, mainCalls);
        Assert.AreEqual(1, finallyCalls);
        Assert.AreEqual(0, exception);
    }
        
    [Test]
    public async Task DispatchAndWrapAsyncBase_calls_error_callback_on_exception()
    {
        // Arrange
        var exception = 0;

        // Act 
        _ = await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () =>
            {
                if (exception == 0)
                {
                    // only to be sure to get the correct signature
                    throw new Exception();
                }
                return 1;
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_calls_error_callback_on_exception_base_signature()
    {
        // Arrange
        var exception = 0;

        // Act 
        void Callback() => throw new Exception();

        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            Callback,
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Exception_Finally()
    {
        // Arrange
        var finallyCalls = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () => throw new Exception(),
            _ => Interlocked.Increment(ref exception), // On exception
            () => Interlocked.Increment(ref finallyCalls)).ConfigureAwait(false); // On finally

        // Assert
        Assert.AreEqual(1, finallyCalls);
        Assert.AreEqual(1, exception);
    }


    [Test]
    public async Task DispatchAndWrapAsyncBase_Double_Exception_Finally()
    {
        // Arrange
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () => throw new Exception(),
            _ => Interlocked.Increment(ref exception), // On exception
            () => throw new Exception()).ConfigureAwait(false); // On finally

        // Assert
        Assert.AreEqual(2, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_Async_Action()
    {
        // Arrange
        var count = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            async () =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                Interlocked.Increment(ref count);
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_Async_Exception()
    {
        // Arrange
        var count = 0;
        var exception = 0;

        // Act 
        await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            async () =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                Interlocked.Increment(ref count);
                throw new Exception();
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, count);
        Assert.AreEqual(1, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_With_Return()
    {
        // Arrange
        var exception = 0;

        // Act 
        var res = await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            () => 78,
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(78, res);
        Assert.AreEqual(0, exception);
    }

    [Test]
    public async Task DispatchAndWrapAsyncBase_Nominal_Async_With_Return()
    {
        // Arrange
        var exception = 0;

        // Act 
        var res = await AsyncWrapperBase.DispatchAndWrapAsyncBase(
            async () =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return 78;
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(78, res);
        Assert.AreEqual(0, exception);
    }
        
    [Test]
    public async Task WrapAsync_call_exception_callback_on_error()
    {
        // Arrange
        var exception = 0;

        // Act 
        await AsyncWrapperBase.WrapAsync(
            async () =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                if (exception == 0)
                {
                    throw new Exception();
                }
                return 78;
            },
            _ => Interlocked.Increment(ref exception)).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, exception);
    }
}
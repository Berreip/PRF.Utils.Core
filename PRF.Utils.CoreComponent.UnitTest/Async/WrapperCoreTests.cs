using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponent.UnitTest.Async
{
    [TestFixture]
    internal sealed class WrapperCoreTests
    {
        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                () => Interlocked.Increment(ref count),
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_Exception()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                () =>
                {
                    Interlocked.Increment(ref count);
                    throw new Exception();
                },
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_Finally()
        {
            // Arrange
            var mainCalls = 0;
            var finallyCalls = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                () =>
                {
                    Interlocked.Increment(ref mainCalls);
                },
                e => Interlocked.Increment(ref exception), // On exception
                () => Interlocked.Increment(ref finallyCalls)); // On finally

            // Assert
            Assert.AreEqual(1, mainCalls);
            Assert.AreEqual(1, finallyCalls);
            Assert.AreEqual(0, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Exception_Finally()
        {
            // Arrange
            var finallyCalls = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                () =>
                {
                    throw new Exception();
                },
                e => Interlocked.Increment(ref exception), // On exception
                () => Interlocked.Increment(ref finallyCalls)); // On finally

            // Assert
            Assert.AreEqual(1, finallyCalls);
            Assert.AreEqual(1, exception);
        }


        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Double_Exception_Finally()
        {
            // Arrange
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                () =>
                {
                    throw new Exception();
                },
                e => Interlocked.Increment(ref exception), // On exception
                () =>
                {
                    throw new Exception();
                }); // On finally

            // Assert
            Assert.AreEqual(2, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_Async_Action()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                async () =>
                {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                },
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(1, count);
            Assert.AreEqual(0, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_Async_Exception()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await WrapperCore.DispatchAndWrapAsyncBase(
                async () =>
                {
                    await Task.Delay(50);
                    Interlocked.Increment(ref count);
                    throw new Exception();
                },
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_With_Return()
        {
            // Arrange
            var exception = 0;

            // Act 
            var res = await WrapperCore.DispatchAndWrapAsyncBase(
                () =>
                {
                    return 78;
                },
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(78, res);
            Assert.AreEqual(0, exception);
        }

        [Test]
        public async Task WrapperCore_DispatchAndWrapAsyncBase_Nominal_Async_With_Return()
        {
            // Arrange
            var exception = 0;

            // Act 
            var res = await WrapperCore.DispatchAndWrapAsyncBase(
                async () =>
                {
                    await Task.Delay(50);
                    return 78;
                },
                e => Interlocked.Increment(ref exception));

            // Assert
            Assert.AreEqual(78, res);
            Assert.AreEqual(0, exception);
        }
    }
}

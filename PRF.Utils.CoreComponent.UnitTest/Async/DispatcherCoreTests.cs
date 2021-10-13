using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponent.UnitTest.Async
{
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
                e => Interlocked.Increment(ref exception));

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
                null);

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
                });

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
                e => Interlocked.Increment(ref exception));

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
                e => Interlocked.Increment(ref exception), // On exception
                () => Interlocked.Increment(ref finallyCalls)); // On finally

            // Assert
            Assert.AreEqual(1, mainCalls);
            Assert.AreEqual(1, finallyCalls);
            Assert.AreEqual(0, exception);
        }

        [Test]
        public async Task DispatchAndWrapAsyncBase_Exception_Finally()
        {
            // Arrange
            var finallyCalls = 0;
            var exception = 0;

            // Act 
            await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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
        public async Task DispatchAndWrapAsyncBase_Double_Exception_Finally()
        {
            // Arrange
            var exception = 0;

            // Act 
            await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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
        public async Task DispatchAndWrapAsyncBase_Nominal_Async_Action()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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
        public async Task DispatchAndWrapAsyncBase_Nominal_Async_Exception()
        {
            // Arrange
            var count = 0;
            var exception = 0;

            // Act 
            await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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
        public async Task DispatchAndWrapAsyncBase_Nominal_With_Return()
        {
            // Arrange
            var exception = 0;

            // Act 
            var res = await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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
        public async Task DispatchAndWrapAsyncBase_Nominal_Async_With_Return()
        {
            // Arrange
            var exception = 0;

            // Act 
            var res = await AsyncWrapperBase.DispatchAndWrapAsyncBase(
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

using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}

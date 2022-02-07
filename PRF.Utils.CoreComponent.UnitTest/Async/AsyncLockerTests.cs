using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Async;
// ReSharper disable UnusedVariable

namespace PRF.Utils.CoreComponent.UnitTest.Async
{
    [TestFixture]
    internal sealed class AsyncLockerTests
    {
        [Test]
        [Timeout(500)]
        public void AsyncLocker_WaitLock_no_deadlock_case()
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;

            //Act
            using (var key = locker.WaitLock())
            {
                i++;
            }

            //Assert
            Assert.AreEqual(1, i);
        }
        
        [Test]
        [Timeout(500)]
        public async Task AsyncLocker_WaitLockAsync_no_deadlock_case()
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;

            //Act
            using (var key = await locker.WaitLockAsync())
            {
                i++;
            }

            //Assert
            Assert.AreEqual(1, i);
        }

        [Test]
        [Timeout(500)]
        public void AsyncLocker_WaitLock_nominal_usage()
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;

            //Act
            Parallel.For(0, 1_000, _ =>
            {
                using (var key = locker.WaitLock())
                {
                    i++;
                }
            });

            //Assert
            Assert.AreEqual(1_000, i);
        }
        
        [Test]
        [Timeout(500)]
        public async Task AsyncLocker_WaitLockAsync_nominal_usage()
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;
            var tasks = Enumerable.Range(0, 1_000).Select(o => Task.Run(async () =>
            {
                using (var key = await locker.WaitLockAsync().ConfigureAwait(false))
                {
                    i++;
                }
            })).ToArray();

            //Act
            await Task.WhenAll(tasks);

            //Assert
            Assert.AreEqual(1_000, i);
        }
        
        [Test]
        [Timeout(500)]
        public void AsyncLocker_Double_dispose_check()
        {
            //Arrange
            var locker = new AsyncLocker();

            //Act
            using (var key = locker.WaitLock())
            {
                key.Dispose();
            }

            //Assert
            Assert.Pass();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Async;

// ReSharper disable UnusedVariable

namespace PRF.Utils.CoreComponent.UnitTest.Async;

public sealed class AsyncLockerTests
{
    [Fact]
    public void AsyncLocker_WaitLock_no_deadlock_case()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(5), () =>
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
            Assert.Equal(1, i);
        });
    }

    [Fact]
    public async Task AsyncLocker_WaitLockAsync_no_deadlock_case()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(5), async () =>
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;

            //Act
            using (var key = await locker.WaitLockAsync().ConfigureAwait(false))
            {
                i++;
            }

            //Assert
            Assert.Equal(1, i);
        }).ConfigureAwait(true);
    }

    [Fact]
    public void AsyncLocker_WaitLock_nominal_usage()
    {
        XunitTimeout.Timeout(TimeSpan.FromSeconds(1), () =>
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;

            //Act
            Parallel.For(0, 100, _ =>
            {
                using (var key = locker.WaitLock())
                {
                    i++;
                }
            });

            //Assert
            Assert.Equal(100, i);
        });
    }

    [Fact]
    public async Task AsyncLocker_WaitLockAsync_nominal_usage()
    {
        await XunitTimeout.TimeoutAsync(TimeSpan.FromSeconds(1), async () =>
        {
            //Arrange
            var locker = new AsyncLocker();
            var i = 0;
            var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(async () =>
            {
                using (var key = await locker.WaitLockAsync().ConfigureAwait(false))
                {
                    i++;
                }
            })).ToArray();

            //Act
            await Task.WhenAll(tasks).ConfigureAwait(false);

            //Assert
            Assert.Equal(100, i);
        }).ConfigureAwait(true);
    }

    [Fact]
    public void AsyncLocker_Double_dispose_check()
    {
        XunitTimeout.Timeout(TimeSpan.FromMilliseconds(500), () =>
        {
            //Arrange
            var locker = new AsyncLocker();

            //Act
            using (var key = locker.WaitLock())
            {
            }

            //Assert
            // OK
        });
    }
}
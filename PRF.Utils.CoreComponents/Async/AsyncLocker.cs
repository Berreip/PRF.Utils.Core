using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// a locker that allow async lock (be very carrefull with this kind of lock as a deadlock is very common)
    /// </summary>
    public interface IAsyncLocker
    {
        /// <summary>
        /// wait a new locker by blocking the current thread IT HAS TO BE USED IN A USING STATEMENT AS IT IS THE DISPOSE THAT RELEASE THE LOCK
        /// </summary>
        IAsyncLock WaitLock();
        
        /// <summary>
        /// wait a new locker in async (do not block current thread) IT HAS TO BE USED IN A USING STATEMENT AS IT IS THE DISPOSE THAT RELEASE THE LOCK
        /// </summary>
        Task<IAsyncLock> WaitLockAsync();
    }

    /// <inheritdoc />
    public sealed class AsyncLocker : IAsyncLocker
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        /// <summary>
        /// Create a new AsyncLocker (prefer keeping it a a readonly field) that allow requesting for new lock 
        /// </summary>
        public AsyncLocker()
        {
            _semaphoreSlim = new SemaphoreSlim(1);
        }

        /// <inheritdoc />
        public IAsyncLock WaitLock()
        {
            return new AsyncLock(_semaphoreSlim).Wait();
        }

        /// <inheritdoc />
        public async Task<IAsyncLock> WaitLockAsync()
        {
            return await new AsyncLock(_semaphoreSlim).WaitAsync().ConfigureAwait(false);
        }

        private sealed class AsyncLock : IAsyncLock
        {
            private readonly SemaphoreSlim _semaphoreSlim;
            private bool _disposed;

            public AsyncLock(SemaphoreSlim semaphoreSlim)
            {
                _semaphoreSlim = semaphoreSlim;
            }


            public IAsyncLock Wait()
            {
                _semaphoreSlim.Wait();
                return this;
            }
            
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }
                _semaphoreSlim.Release();
                _disposed = true;
            }

            public async Task<IAsyncLock> WaitAsync()
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                return this;
            }
        }
    }

    /// <summary>
    /// A lock object to use in a USING that will act as a lock key
    /// </summary>
    public interface IAsyncLock : IDisposable
    {
    }
}
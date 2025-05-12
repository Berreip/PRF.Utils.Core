using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async.TaskPool
{
    /// <summary>
    /// Object containing infos about the task to execute
    /// </summary>
    public interface IWorkInProgress
    {
        /// <summary>
        /// Wait for the work to finish 
        /// </summary>
        Task WaitAsync();

        /// <summary>
        /// Request the work's cancellation
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Work shared base class
    /// </summary>
    internal abstract class WorkBase : IWorkInProgress, IDisposable
    {
        private bool _disposed;
        protected readonly TaskCompletionSource<bool> _tcs;
        protected readonly CancellationTokenSource _cts;

        protected WorkBase()
        {
            _cts = new CancellationTokenSource();
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public bool IsCancellationRequested => _cts.IsCancellationRequested;

        /// <inheritdoc />
        public async Task WaitAsync()
        {
            await _tcs.Task.ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Cancel()
        {
            // handle the case where the token
            // is disposed before cancellation.
            // (TaskCompletionSource does not handle this case)
            try
            {
                if (_disposed)
                {
                    return;
                }
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // already disposed
            }

            _tcs.TrySetCanceled();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // dispose managed state (managed objects).
            }

            _cts.Dispose();
            _disposed = true;
        }

        ~WorkBase()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Represent a work with an async callback
    /// </summary>
    internal sealed class WorkAsync : WorkBase
    {
        private readonly Func<CancellationToken, Task> _workToDoAsync;

        public WorkAsync(Func<CancellationToken, Task> workToDoAsync)
        {
            _workToDoAsync = workToDoAsync;
        }

        /// <summary>
        /// Start the async work
        /// </summary>
        public async Task DoWorkAsync()
        {
            try
            {
                await _workToDoAsync.Invoke(_cts.Token).ConfigureAwait(false);
                _tcs.TrySetResult(true);
            }
            catch (OperationCanceledException)
            {
                _tcs.TrySetCanceled();
            }
            catch (Exception e)
            {
                _tcs.SetException(e);
            }
        }
    }

    /// <summary>
    /// Represent a work with a sync callback
    /// </summary>
    internal sealed class Work : WorkBase
    {
        private readonly Action<CancellationToken> _workToDo;

        public Work(Action<CancellationToken> workToDo)
        {
            _workToDo = workToDo;
        }

        /// <summary>
        /// Start the sync work
        /// </summary>
        public void DoWork()
        {
            try
            {
                _workToDo.Invoke(_cts.Token);
                _tcs.TrySetResult(true);
            }
            catch (OperationCanceledException)
            {
                _tcs.TrySetCanceled();
            }
            catch (Exception e)
            {
                _tcs.SetException(e);
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async.TaskPool
{
    public static class TaskPoolSizedCappedExtensions
    {
        /// <summary>
        /// Do a blocking wait on the underlying work
        /// </summary>
        public static void Wait(this IWorkInProgress work)
        {
            work.WaitAsync().Wait();
        }
        
        /// <summary>
        /// Do a blocking wait on the underlying work and return true if the task has finish before the timeout
        /// </summary>
        public static bool Wait(this IWorkInProgress work, TimeSpan timeout)
        {
            return work.WaitAsync(timeout).Result;
        }

        /// <summary>
        /// Do an async wait with a timeout and return true if the task has finish before the timeout
        /// </summary>
        public static async Task<bool> WaitAsync(this IWorkInProgress work, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            var isTimeoutReached = false;

            // wait either the task or the timeout in a race condition
            await Task.WhenAny(
                work.WaitAsync(),
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(timeout, cts.Token).ConfigureAwait(false);
                        if (!cts.IsCancellationRequested)
                        {
                            isTimeoutReached = true;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // normal cancellation f task ends before
                    }
                }, cts.Token)).ConfigureAwait(false);

            // cancel if needed the timeout task
            cts.Cancel();
            return !isTimeoutReached;
        }
    }
}
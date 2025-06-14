﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMethodReturnValue.Global

namespace PRF.Utils.CoreComponents.Async.TaskPool
{
    /// <summary>
    /// Extensions methods on TaskPoolSized capped class
    /// </summary>
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
            using (var cts = new CancellationTokenSource())
            {
                var isTimeoutReached = false;

                var token = cts.Token;
                // wait either the task or the timeout in a race condition
                var task = await Task.WhenAny(
                    work.WaitAsync(),
                    // ReSharper disable once RedundantCast
                    Task.Run((Func<Task>)(async () =>
                    {
                        try
                        {
                            await Task.Delay(timeout, token).ConfigureAwait(false);
                            if (!token.IsCancellationRequested)
                            {
                                isTimeoutReached = true;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // normal cancellation if task ends before
                        }
                    }), cts.Token)).ConfigureAwait(false);

                // await the result to ensure that the WhenAny throw if an exception arise
                // (otherwise, the result is a faulted task with an exception, but it does not throw the error)
                await task.ConfigureAwait(false);

                // cancel if needed the timeout task
                cts.Cancel();
                return !isTimeoutReached;
            }
        }

        /// <summary>
        /// Wait that all IWorkInProgress complete
        /// </summary>
        public static async Task WaitAllAsync(this IEnumerable<IWorkInProgress> allWorksInProgress)
        {
            await Task
                .WhenAll(allWorksInProgress.Select(async o => await o.WaitAsync().ConfigureAwait(false)).ToArray())
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Do an async parallel execution of ASYNC callbacks capped by the maximum number of thread allowed by the provided ITaskPoolSizeCapped
        /// </summary>
        public static async Task ParallelForEachSizedCappedAsync<T>(this ITaskPoolSizeCapped tpsc, IEnumerable<T> items, Func<T, Task> callbackAsync)
        {
            var allWorksInProgress = new List<IWorkInProgress>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in items)
            {
                allWorksInProgress.Add(tpsc.AddWork(async _ => await callbackAsync(item).ConfigureAwait(false)));
            }

            await allWorksInProgress.WaitAllAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Do an async parallel execution of SYNC callbacks capped by the maximum number of thread allowed by the provided ITaskPoolSizeCapped
        /// </summary>
        public static async Task ParallelForEachSizedCappedAsync<T>(this ITaskPoolSizeCapped tpsc, IEnumerable<T> items, Action<T> callbackSync)
        {
            var allWorksInProgress = new List<IWorkInProgress>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in items)
            {
                allWorksInProgress.Add(tpsc.AddWork(_ => callbackSync(item)));
            }

            await allWorksInProgress.WaitAllAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        public static void ParallelForEachSizedCapped<T>(this ITaskPoolSizeCapped tpsc, IEnumerable<T> items, Action<T> callbackSync)
        {
            // each callback here is sync so it is possible to call a wait here and still limit any deadlock or performance issue.
            tpsc.ParallelForEachSizedCappedAsync(items: items, callbackSync: callbackSync).Wait();
        }

        /// <summary>
        /// Do an async parallel execution of ASYNC callbacks capped by the maximum number of thread given as poolMaximumSize
        /// </summary>
        /// <example>
        ///await new List{Item}().ParallelForEachSizedCappedAsync(items, async (Item obj) =>
        ///{
        ///    await (Do smthg)
        ///}).ConfigureAwait(false);
        /// </example>
        public static async Task ParallelForEachSizedCappedAsync<T>(this IEnumerable<T> items, int poolMaximumSize, Func<T, Task> callbackAsync)
        {
            await new TaskPoolSizeCapped(poolMaximumSize).ParallelForEachSizedCappedAsync(items: items, callbackAsync: callbackAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// Do an async parallel execution of SYNC callbacks capped by the maximum number of thread given as poolMaximumSize
        /// </summary>
        /// <example>
        ///await new List{Item}().ParallelForEachSizedCappedAsync(items, (Item obj) =>
        ///{
        ///    Do smthg
        ///}).ConfigureAwait(false);
        /// </example>
        public static async Task ParallelForEachSizedCappedAsync<T>(this IEnumerable<T> items, int poolMaximumSize, Action<T> callbackSync)
        {
            await new TaskPoolSizeCapped(poolMaximumSize).ParallelForEachSizedCappedAsync(items: items, callbackSync: callbackSync).ConfigureAwait(false);
        }

        /// <summary>
        /// Do an parallel execution of SYNC callbacks capped by the maximum number of thread given as poolMaximumSize
        /// </summary>
        /// <example>
        ///await new List{Item}().ParallelForEachSizedCappedAsync(items, (Item obj) =>
        ///{
        ///    Do smthg
        ///}).ConfigureAwait(false);
        /// </example>
        public static void ParallelForEachSizedCapped<T>(this IEnumerable<T> items, int poolMaximumSize, Action<T> callbackSync)
        {
            new TaskPoolSizeCapped(poolMaximumSize).ParallelForEachSizedCapped(items: items, callbackSync: callbackSync);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMemberInSuper.Global

namespace PRF.Utils.CoreComponents.Async.TaskPool
{
    /// <summary>
    /// Tool to manage the execution of multiple tasks with a maximum number of simultaneous execution
    /// </summary>
    public interface ITaskPoolSizeCapped
    {
        /// <summary>
        /// Add synchronous work to the TaskPool stack that will be executed as soon as possible 
        /// </summary>
        IWorkInProgress AddWork(Action<CancellationToken> workToDo);

        /// <summary>
        /// Add asynchronous work to the TaskPool stack that will be executed as soon as possible 
        /// </summary>
        IWorkInProgress AddWork(Func<CancellationToken, Task> workToDoAsync);

        /// <summary>
        /// Wait for the TaskPoolSizeCapped to be idle (all work currently given to it are completed)
        /// (WARNING : new work can still be added while waiting and no exception from a specific task will be retrieved)
        /// If you want a waitAll for a batch of tasks, use provided .ParallelForEachSizedCappedAsync extension methods. 
        /// </summary>
        Task WaitIdleAsync();
    }

    /// <inheritdoc />
    public sealed class TaskPoolSizeCapped : ITaskPoolSizeCapped
    {
        private readonly int _poolMaximumSize;
        private readonly WorkRunner[] _runners;
        private readonly ConcurrentQueue<WorkBase> _pendingWorks;

        /// <summary>
        /// Creates a new TaskPoolSizeCapped with the specified maximum number of simultaneous execution 
        /// </summary>
        /// <param name="poolMaximumSize">The maximum number of simultaneous execution</param>
        public TaskPoolSizeCapped(int poolMaximumSize)
        {
            if (poolMaximumSize < 1)
                throw new ArgumentException($"The minimum number of simultaneous tasks can not be less than 1 (requested : {poolMaximumSize}).");

            _poolMaximumSize = poolMaximumSize;
            _pendingWorks = new ConcurrentQueue<WorkBase>();
            _runners = Enumerable
                .Range(0, poolMaximumSize)
                .Select(_ => new WorkRunner(_pendingWorks))
                .ToArray();
        }

        /// <inheritdoc />
        public IWorkInProgress AddWork(Action<CancellationToken> workToDo) => TryAddInRunner(new Work(workToDo));

        /// <inheritdoc />
        public IWorkInProgress AddWork(Func<CancellationToken, Task> workToDoAsync) => TryAddInRunner(new WorkAsync(workToDoAsync));

        /// <inheritdoc />
        public async Task WaitIdleAsync()
        {
            await Task.WhenAll(_runners.Select(o => o.RunnerTask).ToArray()).ConfigureAwait(false);
        }

        private WorkBase TryAddInRunner(WorkBase work)
        {
            _pendingWorks.Enqueue(work);
            for (var i = 0; i < _poolMaximumSize; i++)
            {
                // for each runner, we check if it has been started by the current query.
                var hasBeenStarted = _runners[i].Resume();
                if (hasBeenStarted)
                {
                    // if it is, it means that the work we enqueue at least will be handled and we leave
                    break;
                }
                // BUT If not, we continue to iterate because the current runner may be processing
                // a long-running task while others are available
            }

            return work;
        }

        private sealed class WorkRunner
        {
            private int _key;
            private readonly ConcurrentQueue<WorkBase> _queue;
            public Task RunnerTask { get; private set; } = Task.CompletedTask;

            public WorkRunner(ConcurrentQueue<WorkBase> queue)
            {
                _queue = queue;
            }

            public bool Resume()
            {
                if (Interlocked.CompareExchange(ref _key, 1, 0) != 0)
                {
                    return false;
                }

                RunnerTask = Task.Run(async () =>
                {
                    // while true is used to allow retry when the queue is not empty or the freeing may not be proceed
                    while (true)
                    {
                        // We dequeue everything we can :
                        while (_queue.TryDequeue(out var workPending))
                        {
                            using (workPending)
                            {
                                // if cancellation requested, go to the next one
                                if (!workPending.IsCancellationRequested)
                                {
                                    switch (workPending)
                                    {
                                        case Work work:
                                            work.DoWork();
                                            break;

                                        case WorkAsync workAsync:
                                            await workAsync.DoWorkAsync().ConfigureAwait(false);
                                            break;
                                    }
                                }
                            }
                        }

                        // when done, reset the key to be idle again using Interlocked.Exchange
                        // to avoid memory reordering
                        Interlocked.Exchange(ref _key, 0);

                        // if the queue is empty => leave
                        if (_queue.IsEmpty)
                        {
                            break;
                        }

                        // if the queue is not empty, but we are unable to acquire the lock again, we leave (another runner is starting)
                        if (Interlocked.CompareExchange(ref _key, 1, 0) != 0)
                        {
                            break;
                        }

                        // else we have the lock for our runner so we do another cycle
                    }
                });

                return true;
            }
        }
    }
}
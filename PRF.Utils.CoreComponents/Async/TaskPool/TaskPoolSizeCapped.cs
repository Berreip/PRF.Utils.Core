using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        /// Wait for the TaskPoolSizeCapped to be iddle (all work currently given to it are completed)
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
            _runners = Enumerable.Range(0, poolMaximumSize).Select(o => new WorkRunner(_pendingWorks)).ToArray();
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
            // the number ot iteration depends on the current queue count: we resume worker if its index is greater than the remaining work count
            // it avoid to resume too much workers if there is not enought work to do
            var iteration = Math.Min(_poolMaximumSize, _pendingWorks.Count);
            for (var i = 0; i < iteration; i++)
            {
               _runners[i].Resume();
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

            public void Resume()
            {
                if (Interlocked.CompareExchange(ref _key, 1, 0) == 0)
                {
                    RunnerTask = Task.Run(async () =>
                    {
                        while (_queue.TryDequeue(out var workPending))
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
                        // when done, reset the key
                        _key = 0;
                    });
                }
            }
        }
    }
}
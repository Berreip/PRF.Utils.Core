using System;
using System.Threading;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async;

/// <summary>
/// A timer supporting synchronous or asynchronous callbacks, ensuring no reentrancy occurs.
/// The period is calculated as the interval between the end of the previous execution
/// and the start of the next execution.
///
/// Key Features:
/// - Supports sync and async callbacks.
/// - Ensures the callback does not run concurrently (no reentrancy).
/// - Period adjustments include callback execution time.
/// - Handles and exposes exceptions via an event.
/// </summary>
public sealed class TimerSlim : IDisposable
{
    private readonly Timer _timer;
    private bool _disposed;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Event triggered when an exception occurs during callback execution.
    /// The exception is passed to the subscribers for handling.
    /// </summary>
    public event Action<Exception> OnError;

    private readonly Action<CancellationToken> _syncCallback;
    private readonly Func<CancellationToken, Task> _asyncCallback;
    private readonly int _periodMs;

    /// <summary>
    /// Initializes a new instance of TimerSlim with a synchronous callback
    /// </summary>
    /// <param name="callback">Synchronous callback method</param>
    /// <param name="periodMs">Interval between executions in milliseconds</param>
    /// <param name="runImmediately">Whether to run the first execution immediately</param>
    public TimerSlim(Action callback, int periodMs, bool runImmediately = false) : this(_ => callback(), periodMs, runImmediately)
    {
    }

    /// <summary>
    /// Initializes a new instance of TimerSlim with a synchronous callback that support cancellation token
    /// </summary>
    /// <param name="callback">Synchronous callback method</param>
    /// <param name="periodMs">Interval between executions in milliseconds</param>
    /// <param name="runImmediately">Whether to run the first execution immediately</param>
    public TimerSlim(Action<CancellationToken> callback, int periodMs, bool runImmediately = false) : this(periodMs, runImmediately)
    {
        _syncCallback = callback;
    }

    /// <summary>
    /// Initializes a new instance of TimerSlim with an asynchronous callback
    /// </summary>
    /// <param name="asyncCallback">Asynchronous callback method</param>
    /// <param name="periodMs">Interval between executions in milliseconds</param>
    /// <param name="runImmediately">Whether to run the first execution immediately</param>
    public TimerSlim(Func<Task> asyncCallback, int periodMs, bool runImmediately = false) : this(async _ => await asyncCallback().ConfigureAwait(false), periodMs, runImmediately)
    {
    }

    /// <summary>
    /// Initializes a new instance of TimerSlim with an asynchronous callback that support cancellation token
    /// </summary>
    /// <param name="asyncCallback">Asynchronous callback method</param>
    /// <param name="periodMs">Interval between executions in milliseconds</param>
    /// <param name="runImmediately">Whether to run the first execution immediately</param>
    public TimerSlim(Func<CancellationToken, Task> asyncCallback, int periodMs, bool runImmediately = false) : this(periodMs, runImmediately)
    {
        _asyncCallback = asyncCallback ?? throw new ArgumentNullException(nameof(asyncCallback));
    }

    private TimerSlim(int periodMs, bool runImmediately = false)
    {
        _periodMs = periodMs;
        _timer = new Timer(
            callback: TimerCallback,
            state: null,
            dueTime: runImmediately ? 0 : periodMs,
            period: Timeout.Infinite // NO repeat : Manual scheduling for non-reentrant execution.
        );
    }

    /// <summary>
    /// Internal handler for the timer callback. Executes the provided callback (sync or async)
    /// and schedules the next execution, unless the timer has been disposed or stopped.
    /// </summary>
    private async void TimerCallback(object state)
    {
        try
        {
            // execute only if not started
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_asyncCallback != null)
                {
                    await _asyncCallback.Invoke(_cancellationTokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    _syncCallback?.Invoke(_cancellationTokenSource.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful cancellation, no action needed.
        }
        catch (Exception ex)
        {
            // Handle callback exceptions as needed
            RaiseOnError(ex);
        }
        finally
        {

            try
            {
                ScheduleNextExecution();
            }
            // catch if timer is already disposed (after error handling for instance)
            catch (ObjectDisposedException ex)
            {
                RaiseOnError(ex);
            }
        }
    }

    /// <summary>
    /// Schedules the next execution by reconfiguring the timer.
    /// Ensures the next tick is delayed by the specified period.
    /// </summary>
    private void ScheduleNextExecution()
    {
        if (!_cancellationTokenSource.IsCancellationRequested && !_disposed)
        {
            _timer.Change(_periodMs, Timeout.Infinite);
        }
    }

    /// <summary>
    /// Stops the timer and releases all resources.
    /// Further executions will be prevented, and all callbacks are cancelled.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Releases the resources used by the timer.
    /// </summary>
    /// <param name="disposing">Indicates if the method is called by user code or by the finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            // Free managed resources
            _cancellationTokenSource.Cancel();
            _timer.Dispose();
            _cancellationTokenSource.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Invokes the OnError event, passing the given exception to subscribers.
    /// </summary>
    /// <param name="obj">The exception to raise in the event.</param>
    private void RaiseOnError(Exception obj)
    {
        OnError?.Invoke(obj);
    }
}
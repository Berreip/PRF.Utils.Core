using System;
using System.Threading;

namespace PRF.Utils.CoreComponents.Async;

/// <summary>
/// Represent a Queue that allow up to pool items and execute a specific callback when a batch
/// of items reach the size limit OR if the timeout limit is reached,
/// depending on the first fulfilled condition.
/// </summary>
public sealed class BatchProcessingQueue<T> : IDisposable
{
    private readonly int _pageMaximumSize;
    private readonly TimeSpan _timeout;
    private readonly Action<T[]> _onFlushCallBack;
    private T[] _currentPage;
    private int _currentIndex;
    private readonly object _key = new object();
    private readonly Timer _timer;
    private bool _disposed;

    /// <summary>
    /// Create a new Batch processing queue
    /// </summary>
    /// <param name="pageMaximumSize">the maximum size of the page before flushing automatically. It could be set to Int.MaxValue if no page limit is wanted</param>
    /// <param name="timeout">the timeout before flushing the page, even if not full</param>
    /// <param name="onFlushCallBack">The method called to flush the page. WATCH OUT: this method is sync with the timeout OR the last Add so be sure to dispatch it if you do not need it sync</param>
    public BatchProcessingQueue(
        int pageMaximumSize,
        TimeSpan timeout,
        Action<T[]> onFlushCallBack)
    {
        if (pageMaximumSize < 1)
        {
            throw new ArgumentException($"{pageMaximumSize} should be greater thant zero");
        }

        _pageMaximumSize = pageMaximumSize;
        _timeout = timeout;
        _onFlushCallBack = onFlushCallBack;
        _currentPage = new T[pageMaximumSize];
        // set a timer but do not start it yet
        _timer = new Timer(_ => CheckAndProcess(), null, Timeout.InfiniteTimeSpan, timeout);
    }

    /// <summary>
    /// Add a new item and dequeue if limit is reached
    /// </summary>
    public void Add(T item)
    {
        T[] page = null;
        lock (_key)
        {
            if (_currentIndex == 0)
            {
                // as soon as we add at least one item, we start the timer:
                _timer.Change(_timeout, Timeout.InfiniteTimeSpan);
            }

            _currentPage[_currentIndex] = item;
            _currentIndex++;
            if (_currentIndex == _pageMaximumSize)
            {
                page = _currentPage;
                ResetPage();
            }
        }

        if (page != null)
        {
            _onFlushCallBack(page);
        }
    }

    /// <summary>
    /// force flushing of items currently in the queue
    /// </summary>
    public void ForceFlush()
    {
        CheckAndProcess();
    }

    private void CheckAndProcess()
    {
        T[] page = null;
        lock (_key)
        {
            if (_currentIndex != 0)
            {
                Array.Resize(ref _currentPage, _currentIndex);
                page = _currentPage;
                ResetPage();
            }
        }

        if (page != null)
        {
            _onFlushCallBack(page);
        }
    }

    private void ResetPage()
    {
        _currentPage = new T[_pageMaximumSize];
        _currentIndex = 0;
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

        _timer.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// destructor
    /// </summary>
    ~BatchProcessingQueue()
    {
        Dispose(false);
    }
}
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
    private readonly ITimer _timer;
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
        : this(pageMaximumSize, timeout, onFlushCallBack, TimeProvider.System)
    {
    }

    /// <summary>
    /// Create a batch processing queue using the supplied clock for its timeout.
    /// The timeout starts with the first item of each page; subsequent items do not postpone it.
    /// </summary>
    /// <param name="pageMaximumSize">Maximum number of items before flushing a page.</param>
    /// <param name="timeout">Maximum waiting time from the first item; <see cref="Timeout.InfiniteTimeSpan"/> disables automatic timeout flushing.</param>
    /// <param name="onFlushCallBack">Synchronous callback, invoked outside the queue lock. Dispatch asynchronous work from this callback when needed.</param>
    /// <param name="timeProvider">Clock used to create the timer, including a controllable clock for deterministic execution.</param>
    public BatchProcessingQueue(
        int pageMaximumSize,
        TimeSpan timeout,
        Action<T[]> onFlushCallBack,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        if (pageMaximumSize < 1)
        {
            throw new ArgumentException($"{pageMaximumSize} should be greater thant zero");
        }

        _pageMaximumSize = pageMaximumSize;
        _timeout = timeout;
        _onFlushCallBack = onFlushCallBack;
        _currentPage = new T[pageMaximumSize];
        // Le timer est créé inactif : aucune échéance tant que la file reste vide.
        // Add l'arme en mode non périodique au premier élément de chaque page.
        _timer = timeProvider.CreateTimer(OnTimerElapsed, null, Timeout.InfiniteTimeSpan, timeout);
    }

    /// <summary>
    /// Add a new item and dequeue if limit is reached
    /// </summary>
    public void Add(T item)
    {
        T[] page = null;
        lock (_key)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_currentIndex == 0)
            {
                // Le délai part du premier ajout, et non du dernier : un flux continu
                // ne doit pas repousser indéfiniment le traitement d'une page incomplète.
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

    private void OnTimerElapsed(object state)
    {
        CheckAndProcess();
    }

    private void CheckAndProcess()
    {
        T[] page = null;
        lock (_key)
        {
            // Un callback du timer déjà planifié peut arriver après Dispose.
            // La libération abandonne les éléments en attente, elle ne les publie pas.
            if (_disposed)
            {
                return;
            }
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
        // Une page pleine ou un ForceFlush peut devancer l'échéance. On la désarme
        // pour éviter un réveil à vide ; le prochain Add repartira avec son propre délai.
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
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
        lock (_key)
        {
            if (_disposed)
            {
                return;
            }

            // Aucun vidage implicite : appeler ForceFlush avant Dispose si nécessaire.
            _disposed = true;
            // Le finaliseur peut aussi être appelé après un échec du constructeur.
            _timer?.Dispose();
        }
    }

    /// <summary>
    /// destructor
    /// </summary>
    ~BatchProcessingQueue()
    {
        Dispose(false);
    }
}

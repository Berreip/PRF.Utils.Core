using System;
using System.Threading;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// Provide access to a cancellation token and allow to renew the source at any given time in a thread safe way
    /// </summary>
    public sealed class CancellationTokenSourceSafeRenewer : IDisposable
    {
        private readonly object _key = new object();
        private CancellationTokenSource _cts;

        /// <summary>
        /// Return a new CancellationToken and cancel any previous source
        /// </summary>
        public CancellationToken GetNewTokenAndCancelPrevious()
        {
            lock (_key)
            {
                // cancel previous if any
                var previousCts = _cts;
                if (previousCts != null)
                {
                    previousCts.Cancel();
                    previousCts.Dispose();
                }
                // and renew the source
                _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }

        /// <summary>
        /// Return a token linked to the previous source
        /// </summary>
        public CancellationToken GetToken()
        {
            lock (_key)
            {
                // create new source if needed
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                return _cts.Token;
            }
        }

        /// <summary>
        /// Return a token linked to the previous source
        /// </summary>
        public void Cancel()
        {
            lock (_key)
            {
                var previousCts = _cts;
                if (previousCts != null)
                {
                    previousCts.Cancel();
                    previousCts.Dispose();
                    _cts = null;
                }
            }
        }

        /// <summary>
        /// Dispose the CancellationTokenSourceSafeRenewer BUT DO NOT CANCEL IT.
        /// </summary>
        public void Dispose()
        {
            _cts?.Dispose();
        }
    }
}
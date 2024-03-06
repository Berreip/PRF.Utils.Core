using System.Threading;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// Provide an access to a cancellation token and allow to renew the source at any given time in a thread safe way
    /// </summary>
    public sealed class CancellationTokenSourceSafeRenewer
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
                _cts?.Cancel();
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
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;
                }
            }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using PRF.Utils.Tracer.Listener.Helpers;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.Tracer.Listener
{
    /// <inheritdoc />
    /// <summary>
    /// The custom trace listener. It is he who takes care of centralizing the traces and returning pages
    /// traces when the page is full or after a certain Timeout
    /// </summary>
    internal sealed class TraceListenerSync : TraceListener
    {
        private readonly int _poolingPageSize;
        private readonly BlockingCollection<TraceWrapper> _buffer;
        private readonly Timer _timer;
        private readonly Task _dequeueTask;

        /// <summary>
        /// Maximum time in case of full buffer before throwing an exception
        /// </summary>
        private readonly TimeSpan _maxTimeoutForFullBuffer = TimeSpan.FromSeconds(3);

        private readonly object _key = new object();

        /// <summary>
        /// The event raised when sending a page
        /// </summary>
        public event Action<TraceData[]> OnTracesSent;

        /// <summary>
        /// The trace level filtered at the listener level. This approach allows filtering
        /// dynamic even via calls to static Traces.TraceSmtg() methods. Otherwise, only calls
        /// via the TraceSource are filtered.
        /// --------
        /// The consequence is that an additional check must be carried out = slight impact on performance
        /// </summary>
        public TraceEventType DynamicFilter { private get; set; } = TraceEventType.Information;

        /// <inheritdoc />
        /// <summary>
        /// Constructs a plotter with maximum flush time and page size
        /// </summary>
        /// <param name="timeForFlush">the maximum time range from which we will empty the stack despite tt</param>
        /// <param name="poolingPageSize">the maximum size of the pages emitted by the plotter (they can be smaller)</param>
        public TraceListenerSync(TimeSpan timeForFlush, int poolingPageSize)
        {
            ListenerHelpers.CheckEntryValuesAndThrowExceptionIfFailed(timeForFlush, poolingPageSize);

            _poolingPageSize = poolingPageSize;

            // we define the maximum size that the buffer can reach before the trace calls are
            // blocking (case of filling extremely faster than emptying via the Dequeue() task == highly improbable)
            //=> this size is defined as the size of a page == we store a maximum of one page in the buffer
            _buffer = new BlockingCollection<TraceWrapper>(poolingPageSize);

            // Configures the timer managing the maximum page flush time
            _timer = new Timer(timeForFlush.TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            // starts the analysis stack:
            _dequeueTask = Task.Run(Dequeue);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_key)
            {
                if (_buffer.IsAddingCompleted) return;
                // add a forceFlush to the stack
                _buffer.Add(new TraceWrapper());

                // then restart the timer
                _timer.Start();
            }
        }

        private void Dequeue()
        {
            try
            {
                // current page under construction
                var currentPage = new TraceData[_poolingPageSize];
                var currentIndex = 0;
                // ReSharper disable once InconsistentlySynchronizedField => non sens ici
                foreach (var trace in _buffer.GetConsumingEnumerable())
                {
                    if (trace.Action == TraceAction.Trace)
                    {
                        // stores the data in the current page:
                        currentPage[currentIndex] = new TraceData(trace);
                        currentIndex++;

                        if (currentIndex != _poolingPageSize) continue;
                    }

                    // stop the timer
                    _timer.Stop();
                    // if the page is full or we force the flush, we send the page:
                    RaiseSentTraces(currentPage);
                    currentPage = new TraceData[_poolingPageSize];
                    currentIndex = 0;

                    // restarts the timer
                    _timer.Start();
                }

                // stop the timer
                _timer.Stop();

                // once the buffer is closed, we empty the remaining stack:
                EmptyClosedBuffer(currentIndex, currentPage);
            }
            catch (Exception e)
            {
                RaiseException(e);
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void EmptyClosedBuffer(int currentIndex, TraceData[] currentPage)
        {
            // if we completely empty a page, then there is nothing to send
            if (currentIndex == 0) return;

            // otherwise we resize the page according to the number of traces present before sending it
            var adaptedPage = new TraceData[currentIndex];
            for (var i = 0; i < currentIndex; i++)
            {
                adaptedPage[i] = currentPage[i];
            }

            RaiseSentTraces(adaptedPage);
        }

        /// <inheritdoc />
        public override void Write(string message)
        {
            // We trace the simple message for a direct call to Trace.Write(...)
            AddToBuffer(new TraceWrapper(message));
        }

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            // We trace the simple message for a direct call to Trace.WriteLine(...)
            AddToBuffer(new TraceWrapper(message));
        }

        /// <inheritdoc />
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (DynamicFilter < eventType) return; // perf = filter before building the wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, data.ToStringOrNull()));
        }

        /// <inheritdoc />
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (DynamicFilter < eventType) return; // perf = filter before building the wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, data.ToStringOrNullList()));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (DynamicFilter < eventType) return; // perf = filter before building the wrapper
            // adds TraceEvent info if no message
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, $"TraceEvent id:{id}"));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (DynamicFilter < eventType) return; // perf = filter before building the wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, format, args));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (DynamicFilter < eventType) return; // perf = filter before building the wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, message));
        }

        /// <summary>
        /// Raise trace event
        /// </summary>
        private void RaiseSentTraces(TraceData[] traceArray)
        {
            var handler = OnTracesSent;
            // if there is no handler, WE RUN IT WITH NO EXCEPTION because after all, we may be in a context
            // where we do not wish to trace (TU...). It is not the responsibility of the listener to know this.

            // In the same way, the event is raised in SYNCHRONOUS because it is up to the subscriber to manage the asynchronous pattern (very
            // preferable so as not to slow down the tracer) but SURELY NOT for the tracer to decide to be asynchronous.
            handler?.Invoke(traceArray);
        }

        private void AddToBuffer(TraceWrapper trace)
        {
            // the lock will stack the callers (but that's what we want)
            lock (_key)
            {
                if (_buffer.IsAddingCompleted || _buffer.TryAdd(trace)) return;
            }

            // the timed call is not locked and therefore manages the catch for additions even though the collection has been marked as completed (CompleteAdding = true)
            // ReSharper disable InconsistentlySynchronizedField => no lock for this case because we do not want to block the completeAdding too.
            try
            {
                // otherwise, we send an event to signal that the addition could not be made immediately and we try again with a waiting time
                RaiseSentTraces(ListenerHelpers.GetExceptionArray($"unable to add message immediately {trace.Message}: buffer full: {_buffer.Count}"));
                if (!_buffer.TryAdd(trace, _maxTimeoutForFullBuffer))
                {
                    // In the case where we cannot insert a trace at the end of the timeout, we throw an exception
                    throw new ArgumentException(
                        $"ERROR: Max timeout for trace insertion in buffer reached ({_maxTimeoutForFullBuffer.TotalMilliseconds} ms)");
                }
            }
            catch (InvalidOperationException e)
            {
                RaiseSentTraces(ListenerHelpers.GetExceptionArray($"InvalidOperationException (case of the buffer where CompleteAdding = true for example) in the traces. {e}"));
            }
            // ReSharper restore InconsistentlySynchronizedField
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            lock (_key)
            {
                FlushAndCompleteAddingAsync().Wait(TimeSpan.FromSeconds(5));
                _buffer.Dispose();
                _timer.Dispose();
                base.Dispose(disposing);
            }
        }
        /// <summary>
        /// Close the trace buffer and wait for the end of the trace task (via async)
        /// </summary>
        /// <returns>the stain of traces being closed</returns>
        public async Task FlushAndCompleteAddingAsync()
        {
            lock (_key)
            {
                // force the _timer to stop
                _timer.Stop();
                if (!_buffer.IsAddingCompleted)
                {
                    _buffer.CompleteAdding();
                }
            }

            // wait for the collection to close and the trace task to end
            await _dequeueTask.ConfigureAwait(false);
        }

        /// <summary>
        /// In the event of an exception, we send a message to the outside to report the problem
        /// </summary>
        /// <param name="e">the exception</param>
        private void RaiseException(Exception e)
        {
            RaiseSentTraces(ListenerHelpers.GetExceptionArray($"EXCEPTION IN TRACER: UNABLE TO FORMAT {e}"));
        }
    }
}
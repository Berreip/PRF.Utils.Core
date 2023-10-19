using System.Diagnostics;

namespace PRF.Utils.Tracer.Listener.Traces
{
    internal sealed class TraceWrapper
    {
        public string Message { get; }
        public object[] TraceData { get; }
        public TraceEventCache EventCache { get; }
        public TraceEventType EventType { get; }
        public string Source { get; }
        public int Identifier { get; }
        public TraceAction Action { get; }
        /// <summary>
        /// create a TraceWrapper which asks to force the flushing of traces
        /// </summary>
        public TraceWrapper()
        {
            Action = TraceAction.ForceFlush;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a trace TraceWrapper when we only have the message
        /// </summary>
        public TraceWrapper(string message)
            // no traceEventCache == we create one on the current date (ALWAYS IN UTC)
            : this(new TraceEventCache(), string.Empty, TraceEventType.Information, -1, message)
        {
        }
        
        /// <summary>
        /// Create a TraceWrapper when you have detailed information
        /// </summary>
        public TraceWrapper(TraceEventCache eventCache, string source, TraceEventType eventType, int identifier, string message, params object[] traceData)
        {
            EventCache = eventCache;
            EventType = eventType;
            Identifier = identifier;
            Message = message;
            TraceData = traceData;
            Source = source;
            Action = TraceAction.Trace;
        }
    }
}
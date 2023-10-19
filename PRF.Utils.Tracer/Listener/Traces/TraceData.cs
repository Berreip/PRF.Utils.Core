using System.Diagnostics;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PRF.Utils.Tracer.Listener.Traces
{
    /// <summary>
    /// Trace data in the buffer (immutable)
    /// </summary>
    public struct TraceData
    {
        /// <summary>
        /// The trace level
        /// </summary>
        public TraceEventType EventType { get; }

        /// <summary>
        /// The identifier provided by the source (ideally an enum member)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Additional data possibly provided by the caller (may be null)
        /// </summary>
        public object[] AdditionalData { get; }

        /// <summary>
        /// Trace details either provided by the trace method (ideally) or created on the fly during tracing (less precise)
        /// </summary>
        public TraceEventCache EventCache { get; }

        /// <summary>
        /// The trace message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The source of the trace
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Detailed constructor
        /// </summary>
        public TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, params object[] additionalData)
        {
            EventCache = eventCache;
            Message = message;
            Source = source;
            EventType = eventType;
            Id = id;
            AdditionalData = additionalData;
        }

        internal TraceData(TraceWrapper traceWrapper)
        {
            EventCache = traceWrapper.EventCache;
            Message = traceWrapper.Message;
            EventType = traceWrapper.EventType;
            Source = traceWrapper.Source;
            Id = traceWrapper.Identifier;
            AdditionalData = traceWrapper.TraceData;
        }
    }
}
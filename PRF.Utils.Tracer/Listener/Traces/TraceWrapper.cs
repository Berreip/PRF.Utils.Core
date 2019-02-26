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
        /// créer un TraceWrapper qui demande de forcer le flush des traces
        /// </summary>
        public TraceWrapper()
        {
            Action = TraceAction.ForceFlush;
        }

        /// <inheritdoc />
        /// <summary>
        /// Créer un TraceWrapper de trace quand on dispose seulement du message
        /// </summary>
        public TraceWrapper(string message)
            // pas de traceEventCache == on en créer un à la date actuelle (TOUJOURS EN UTC)
            : this(new TraceEventCache(), string.Empty, TraceEventType.Information, -1, message)
        {
        }

        /// <summary>
        /// Créer un TraceWrapper de trace quand on dispose des informations détaillées
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
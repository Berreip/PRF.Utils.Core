using System.Diagnostics;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PRF.Utils.Tracer.Listener.Traces
{
    /// <summary>
    /// Les données de traces dans le buffer (immutables)
    /// </summary>
    public struct TraceData
    {
        /// <summary>
        /// Le niveau de la trace
        /// </summary>
        public TraceEventType EventType { get; }

        /// <summary>
        /// L'identifiant fourni par la source (idéalement un membre d'enum)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Données additionelles éventuellement fournies par l'appelant (peut être null)
        /// </summary>
        public object[] AdditionalData { get; }

        /// <summary>
        /// Détail de la trace soit fourni par la méthode de trace (idéalement) soit créer à la volée lors de la trace (moins précis)
        /// </summary>
        public TraceEventCache EventCache { get; }

        /// <summary>
        /// Le message de la trace
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// La source de la trace
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Constructeur détaillé
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
using System;
using System.Diagnostics;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace PRF.Utils.Tracer.Configuration
{
    /// <summary>
    /// The tracer configuration class
    /// </summary>
    public sealed class TraceConfig
    {
        /// <summary>
        /// The name of the tracer (exactly, that of the traceSource) under which it will be saved
        /// </summary>
        /// <remarks>
        /// We propose a default name: this design decision starts from the premise that the simplicity of using static traces, although
        /// less powerful than traces added directly to the traceSource will be used much more by devs.
        /// Consequently, having several source traces (which is a Microsoft recommendation) will clearly not be clear
        /// since there will be a mixture of static and direct traces in different TraceSources. By proposing a name
        /// by default we group the traceSource under the same name
        /// </remarks>
        public string TraceName { get; set; } = "MainTracerSync";

        /// <summary>
        /// The behavior of the tracer with respect to static calls to Trace.TraceInformation(..)
        /// </summary>
        public TraceStaticBehavior TraceBehavior { get; set; } = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer;

        /// <summary>
        /// The trace level from which we decide to trace (SourceLevels.Information by default)
        /// </summary>
        public SourceLevels TraceLevel { get; set; } = SourceLevels.Information;

        /// <summary>
        /// The maximum time between two flushes even if we do not reach the desired page size
        /// </summary>
        public TimeSpan MaximumTimeForFlush { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// The size of the trace blocks that are reported via the OnTracesSent event. We will empty the
        /// cache as soon as this number (or the timer) is reached
        /// </summary>
        public int PageSize { get; set; } = 10_000;
    }
}
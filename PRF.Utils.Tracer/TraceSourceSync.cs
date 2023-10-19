using System;
using System.Diagnostics;
using System.Threading.Tasks;
using PRF.Utils.Tracer.Configuration;
using PRF.Utils.Tracer.Listener;
using PRF.Utils.Tracer.Listener.Helpers;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.Tracer
{
    /// <inheritdoc cref="TraceSource" />
    /// <summary>
    /// The implementation of a source trace using a TraceListenerSync. This object is a traceSource only
    /// for cases where we decide to inject it and manipulate it directly
    /// </summary>
    public sealed class TraceSourceSync : TraceSource, IDisposable
    {
        private readonly TraceListenerSync _traceListener;
        private readonly TraceStaticBehavior _traceConfigBehavior;

        /// <summary>
        /// The event raised when a trace page has been created
        /// </summary>
        public event Action<TraceData[]> OnTracesSent
        {
            add => _traceListener.OnTracesSent += value;
            remove => _traceListener.OnTracesSent -= value;
        }

        /// <summary>
        /// The trace level from which we trace
        /// </summary>
        public SourceLevels TraceLevel
        {
            get => Switch.Level;
            set => UpdateTraceSourceAndDynamicFilter(value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Constructs a tracer with the default configuration (TraceConfig)
        /// </summary>
        public TraceSourceSync() : this(new TraceConfig())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Constructs a tracer with user-provided configuration
        /// </summary>
        /// <param name="traceConfig">tracer configuration</param>
        public TraceSourceSync(TraceConfig traceConfig) : base(traceConfig.TraceName, traceConfig.TraceLevel)
        {
            _traceListener = new TraceListenerSync(traceConfig.MaximumTimeForFlush, traceConfig.PageSize);

            Listeners.Clear();
            Listeners.Add(_traceListener);
            UpdateTraceSourceAndDynamicFilter(traceConfig.TraceLevel);
            _traceConfigBehavior = traceConfig.TraceBehavior;
            switch (_traceConfigBehavior)
            {
                case TraceStaticBehavior.DoNothing:
                    break;
                case TraceStaticBehavior.AddListenerToStaticAccess:
                    Trace.Listeners.Add(_traceListener);
                    break;
                case TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer:
                    Trace.Listeners.Remove("Default");
                    Trace.Listeners.Add(_traceListener);
                    break;
                case TraceStaticBehavior.AddListenerToStaticAccessAndClearAll:
                    Trace.Listeners.Clear();
                    Trace.Listeners.Add(_traceListener);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_traceConfigBehavior), _traceConfigBehavior, null);
            }
        }

        /// <summary>
        /// Updates the source trace switch but also that of the listener (which is dynamic to be able to switch levels
        /// even on static calls to Trace.Smthg)
        /// </summary>
        private void UpdateTraceSourceAndDynamicFilter(SourceLevels sourceLevels)
        {
            Switch.Level = sourceLevels;
            _traceListener.DynamicFilter = sourceLevels.ToTraceEventType();
        }

        /// <summary>
        /// Puts all traces in the buffer and empties the latter while closing the listener: IT CANNOT
        /// NO MORE RECEIVING NEW TRACKS. The method waits for the buffer to close to return (hence the async)
        /// </summary>
        public async Task FlushAndCompleteAddingAsync()
        {
            _traceListener.Flush();
            await _traceListener.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // arrange the plotter
            _traceListener.Dispose();

            // remove the tracer if necessary
            switch (_traceConfigBehavior)
            {
                case TraceStaticBehavior.DoNothing:
                    break;
                case TraceStaticBehavior.AddListenerToStaticAccess:
                case TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer:
                case TraceStaticBehavior.AddListenerToStaticAccessAndClearAll:
                    Trace.Listeners.Remove(_traceListener);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_traceConfigBehavior), _traceConfigBehavior, null);
            }
        }
    }

    /// <summary>
    /// The behavior of the tracer in relation to static traces: => Trace.TraceInformation("...") etc...
    /// </summary>
    public enum TraceStaticBehavior
    {
        /// <summary>
        /// default value: the traceSource simply creates a Listener and adds it to ITS list of listeners
        /// </summary>
        DoNothing,

        /// <summary>
        /// Adds the Trace Listener to the static Trace.Listener access (so it will be called for each static Trace.TraceSmthg.. call)
        /// </summary>
        AddListenerToStaticAccess,

        /// <summary>
        /// Adds the Trace Listener to the static Trace.Listener access
        /// and first clears the default listener if it is present
        /// => can be a lot more efficient but prevents the reporting of information via Visual Studio output in debug for example
        /// </summary>
        AddListenerToStaticAccessAndRemoveDefaultTracer,
        
        /// <summary>
        /// Adds the Trace Listener to the static Trace.Listener access
        /// and first deletes all present listeners (maximum performance)
        /// => can be a lot more efficient but prevents the reporting of information via Visual Studio output in debug for example
        /// </summary>
        AddListenerToStaticAccessAndClearAll,
    }
}
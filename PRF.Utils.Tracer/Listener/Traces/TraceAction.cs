namespace PRF.Utils.Tracer.Listener.Traces
{
    /// <summary>
    /// Lénumération des actions de traces pour le wrapper interne (rajouter une trace ou demander un flush de force)
    /// </summary>
    internal enum TraceAction
    {
        Trace,
        ForceFlush,
    }
}
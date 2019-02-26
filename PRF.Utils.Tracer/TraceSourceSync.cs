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
    /// L'implémentation d'un trace source utilisant un TraceListenerSync. Cet objet est un traceSource uniquement
    /// pour les cas où l'on décide de l'injecter et de manipuler directement
    /// </summary>
    public sealed class TraceSourceSync : TraceSource, IDisposable
    {
        private readonly TraceListenerSync _traceListener;
        private readonly TraceStaticBehavior _traceConfigBehavior;

        /// <summary>
        /// L'évènement levé lorsqu'une page de trace à été constituée
        /// </summary>
        public event Action<TraceData[]> OnTracesSent
        {
            add => _traceListener.OnTracesSent += value;
            remove => _traceListener.OnTracesSent -= value;
        }

        /// <summary>
        /// Le niveau de trace à partir duquel on trace
        /// </summary>
        public SourceLevels TraceLevel
        {
            get => Switch.Level;
            set => UpdateTraceSourceAndDynamicFilter(value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Construit un traceur avec la configuration par défaut (TraceConfig)
        /// </summary>
        public TraceSourceSync(): this(new TraceConfig()){}
        
        /// <inheritdoc />
        /// <summary>
        /// Construit un traceur avec une configuration fournie par l'utilisateur
        /// </summary>
        /// <param name="traceConfig">la configuration du traceur</param>
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
                case TraceStaticBehavior.AddListenerToStaticAcces:
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
        /// Met à jour le switch du trace Source mais également celui du listener (qui est dynamique pour pouvoir switcher de niveau
        /// même sur les appels statiques à Trace.qqch)
        /// </summary>
        private void UpdateTraceSourceAndDynamicFilter(SourceLevels sourceLevels)
        {
            Switch.Level = sourceLevels;
            _traceListener.DynamicFilter = sourceLevels.ToTraceEventType();

        }

        /// <summary>
        /// Met toutes les traces dans le buffer et vide ce dernier tout en cloturant le listener: IL NE PEUT
        /// PLUS RECEVOIR DE NOUVELLES TRACES. La méthode attends la cloture du buffer pour revenir (d'où le async)
        /// </summary>
        public async Task FlushAndCompleteAddingAsync()
        {
            _traceListener.Flush();
            await _traceListener.FlushAndCompleteAddingAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // dispose le traceur
            _traceListener.Dispose();

            // retire si besoin le traceur
            switch (_traceConfigBehavior)
            {
                case TraceStaticBehavior.DoNothing:
                    break;
                case TraceStaticBehavior.AddListenerToStaticAcces:
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
    /// Le comportement du traceur par rapport aux traces statiques: => Trace.TraceInformation("...") etc...
    /// </summary>
    public enum TraceStaticBehavior
    {
        /// <summary>
        ///  valeur par défaut: le traceSource créer simplement un Listener et l'ajoute dans SA liste de listeners
        /// </summary>
        DoNothing,

        /// <summary>
        /// Ajoute le Listener de traces à l'accès statique Trace.Listener (il sera donc appelé pour chaque appel statique Trace.Traceqqch..)
        /// </summary>
        AddListenerToStaticAcces,

        /// <summary>
        /// Ajoute le Listener de traces à l'accès statique Trace.Listener
        /// et efface préalablement le listener par défaut si il est présent
        /// => peut être bcq plus performant mais empêche les remontées d'infos via l'output Visual Studio en debug par exemple
        /// </summary>
        AddListenerToStaticAccessAndRemoveDefaultTracer,

        /// <summary>
        /// Ajoute le Listener de traces à l'accès statique Trace.Listener
        /// et efface préalablement tous les listeners présents (performances maximum)
        /// => peut être bcq plus performant mais empêche les remontées d'infos via l'output Visual Studio en debug par exemple
        /// </summary>
        AddListenerToStaticAccessAndClearAll
    }
}

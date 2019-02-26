using System;
using System.Diagnostics;

namespace PRF.Utils.Tracer.Configuration
{
    /// <summary>
    /// La classe de configuration du traceur
    /// </summary>
    public sealed class TraceConfig
    {
        /// <summary>
        /// Le nom du traceur (exactement, celui du traceSource) sous lequel il sera sauvegardé 
        /// </summary>
        /// <remarks> 
        /// On propose un nom par défaut: cette décision de design pars du postulat que la simplicité d'utilisation des traces statiques, bien que
        /// moins puissantes que les traces ajoutées directement au traceSource seront bcq plus utilisées par les devs.
        /// En conséquence, avoir plusieurs traces sources (ce qui est une préconisation Microsoft) ne sera clairement pas clair
        /// puisqu'il y aura un mélange de traces statique et directe dans des TracesSources différents. En proposant un nom
        /// par défaut on regroupe les traceSource sous la même appelation
        /// </remarks>
        public string TraceName { get; set; } = @"MainTracerSync";

        /// <summary>
        /// Le comportement du traceur vis à vis des appels statique à Trace.TraceInformation(..)
        /// </summary>
        public TraceStaticBehavior TraceBehavior { get; set; } = TraceStaticBehavior.AddListenerToStaticAccessAndRemoveDefaultTracer;

        /// <summary>
        /// Le niveau de trace à partir duquel on décide de tracer (SourceLevels.Information par défaut)
        /// </summary>
        public SourceLevels TraceLevel { get; set; } = SourceLevels.Information;

        /// <summary>
        /// Le temps maximum entre deux flushs même si on n'atteint pas la taille de la page souhaité
        /// </summary>
        public TimeSpan MaximumTimeForFlush { get; set; } = TimeSpan.FromMilliseconds(500);
       
        /// <summary>
        /// La taille des blocs de traces qui sont remonté via l'évènement OnTracesSent. On videra le
        /// cache dès que ce nombre (ou le timer) est atteint
        /// </summary>
        public int PageSize { get; set; } = 10_000;
    }
}

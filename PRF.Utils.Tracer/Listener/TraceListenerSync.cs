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
    /// Le listener de trace custom. C'est lui qui s'occupe de centraliser les traces et de renvoyer des pages
    /// de traces quand la page est pleine ou au bout d'un certain Timeout
    /// </summary>
    internal sealed class TraceListenerSync : TraceListener
    {
        private readonly int _poolingPageSize;
        private readonly BlockingCollection<TraceWrapper> _buffer;
        private readonly Timer _timer;
        private readonly Task _dequeueTask;

        /// <summary>
        /// Temps maximum en cas de buffer plein avant de lancer une exception
        /// </summary>
        private readonly TimeSpan _maxTimeoutForFullBuffer = TimeSpan.FromSeconds(3);

        private readonly object _key = new object();

        /// <summary>
        /// L'évènement levé lors de l'envoie d'une page
        /// </summary>
        public event Action<TraceData[]> OnTracesSent;

        /// <summary>
        /// Le niveau de traces filtré au niveau du listener. Cette approche permet de faire du filtrage
        /// dynamique même via les appels aux méthodes statiques Traces.TraceQqch(). Sinon, seul les appels
        /// via le TraceSource sont filtrés.
        /// --------
        /// La conséquence c'est quand même qu'une vérification supplémentaire doit être faite = léger impact sur les perfs
        /// </summary>
        public TraceEventType DynamicFilter { private get; set; } = TraceEventType.Information;

        /// <inheritdoc />
        /// <summary>
        /// Construit un traceur avec un temps de flush maximum et une taille de page
        /// </summary>
        /// <param name="timeForFlush">la plage maximum de temps à partir duquel on va vider la pile malgré tt</param>
        /// <param name="poolingPageSize">la taille des page émises par le traceur au maximum (elles peuvent être plus petites)</param>
        public TraceListenerSync(TimeSpan timeForFlush, int poolingPageSize)
        {
            ListenerHelpers.CheckEntryValuesAndThrowExceptionIfFailed(timeForFlush, poolingPageSize);

            _poolingPageSize = poolingPageSize;

            // on défini la taille maximum que pourra atteindre le buffer avant que les appels de trace soient
            // bloquant (cas de remplissage extrèment plus rapide que le vidage via la tache Dequeue() == hautement improbable)
            //=> cette taille est définie à la taille d'une page == on stocke au maximum une page dans le buffer 
            _buffer = new BlockingCollection<TraceWrapper>(poolingPageSize);

            // Configure le timer gérant le temps de flush maximum des pages
            _timer = new Timer(timeForFlush.TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            // démarre la pile d'analyse:
            _dequeueTask = Task.Run(() => Dequeue());
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_key)
            {
                if (_buffer.IsAddingCompleted) return;
                // ajoute un forceFlush dans la pile
                _buffer.Add(new TraceWrapper());

                // puis redémarre le timer
                _timer.Start();
            }
        }

        private void Dequeue()
        {
            try
            {
                // page actuelle en cours de constitution
                var currentpage = new TraceData[_poolingPageSize];
                var currentIndex = 0;
                // ReSharper disable once InconsistentlySynchronizedField => non sens ici
                foreach (var trace in _buffer.GetConsumingEnumerable())
                {
                    if (trace.Action == TraceAction.Trace)
                    {
                        // stocke la donnée dans la page actuelle:
                        currentpage[currentIndex] = new TraceData(trace);
                        currentIndex++;

                        if (currentIndex != _poolingPageSize) continue;
                    }
                    // arrête le timer
                    _timer.Stop();
                    // si la page est pleine ou que l'on force le flush, on envoie la page:
                    RaiseSentTraces(currentpage);
                    currentpage = new TraceData[_poolingPageSize];
                    currentIndex = 0;

                    // redémarre le timer
                    _timer.Start();

                }
                // stoppe le timer
                _timer.Stop();

                // une fois le buffer fermé, on vide la pile restante:
                EmptyClosedBuffer(currentIndex, currentpage);
            }
            catch (Exception e)
            {
                RaiseException(e);
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void EmptyClosedBuffer(int currentIndex, TraceData[] currentPage)
        {
            // si on vide 'tout pile' une page, alors il n'y a rien à envoyer
            if (currentIndex == 0) return;

            // sinon on redimensionne la page en fonction du nombre de traces présentes avant de l'envoyer
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
            // On trace le message simple pour un appel direct à Trace.Write(...)
            AddToBuffer(new TraceWrapper(message));
        }

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            // On trace le message simple pour un appel direct à Trace.WriteLine(...)
            AddToBuffer(new TraceWrapper(message));
        }

        /// <inheritdoc />
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (DynamicFilter < eventType) return; // perf = filtre avant de construire le wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, data.ToStringOrNull()));
        }

        /// <inheritdoc />
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (DynamicFilter < eventType) return; // perf = filtre avant de construire le wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, data.ToStringOrNullList()));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (DynamicFilter < eventType) return; // perf = filtre avant de construire le wrapper
            // rajoute l'info de TraceEvent si aucun message
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, $"TraceEvent id:{id}"));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (DynamicFilter < eventType) return; // perf = filtre avant de construire le wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, format, args));
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (DynamicFilter < eventType) return; // perf = filtre avant de construire le wrapper
            AddToBuffer(new TraceWrapper(eventCache, source, eventType, id, message));
        }

        /// <summary>
        /// Lève l'évènement de trace
        /// </summary>
        private void RaiseSentTraces(TraceData[] traceArray)
        {
            var handler = OnTracesSent;
            // s'il n'y a aucun handler, ON LE LANCE PAS D'EXCEPTION car après tout, on est peut être dans un contexte 
            // où l'on ne souhaite pas tracer (TU...). Ce n'est pas la responsabilité du listener de savoir celà.

            // De la même manière, l'évènement est levé en SYNCHRONE car c'est à l'abonné de gérer l'asynchronisme (très
            // préférable pour ne pas emboliser le traceur) mais SUREMENT PAS au traceur de décider d'être asynchrone.
            handler?.Invoke(traceArray);
        }

        private void AddToBuffer(TraceWrapper trace)
        {
            // le lock va empiler les appelants (mais c'est ce que l'on souhaite)
            lock (_key)
            {
                if (_buffer.IsAddingCompleted || _buffer.TryAdd(trace)) return;
            }

            // l'appel temporisé n'est pas locké et gère donc le catch des ajout alors que la collection a été marquée comme terminée (CompleteAdding = true)
            // ReSharper disable InconsistentlySynchronizedField => pas de lock pour ce cas car on ne souhaite pas bloquer le completeAdding aussi.
            try
            {

                // sinon, on envoie un évènement pour signaler que l'ajout n'a pu être fait immédiatement et on retente avec un temps d'attente
                RaiseSentTraces(ListenerHelpers.GetExceptionArray($"impossible d'ajouter immédiatement le message {trace.Message}: buffer plein : {_buffer.Count}"));
                if (!_buffer.TryAdd(trace, _maxTimeoutForFullBuffer))
                {
                    // Dans le cas où l'on ne peut pas insérer une trace à la fin du timeout, on lance une exception
                    throw new ArgumentException(
                        $@"ERROR: Max timeout for trace insertion in buffer reached ({_maxTimeoutForFullBuffer.TotalMilliseconds} ms)");
                }
            }
            catch (InvalidOperationException e)
            {
                RaiseSentTraces(ListenerHelpers.GetExceptionArray($"InvalidOperationException (cas du buffer où CompleteAdding = true par exemple) dans les traces. {e}"));
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
        /// Cloture le buffer de traces et attends la fin de la tache de traces (via le async)
        /// </summary>
        /// <returns>la tache de traces en train de se cloturer</returns>
        public async Task FlushAndCompleteAddingAsync()
        {
            lock (_key)
            {
                // force le _timer à s'arrêter
                _timer.Stop();
                if (!_buffer.IsAddingCompleted)
                {
                    _buffer.CompleteAdding();
                }
            }

            // attends la cloture de la collection et la fin de la tache de trace
            await _dequeueTask.ConfigureAwait(false);
        }

        /// <summary>
        /// En cas d'exception, on trace un message vers l'extérieur pour signaler le problème
        /// </summary>
        /// <param name="e">l'exception</param>
        private void RaiseException(Exception e)
        {
            RaiseSentTraces(ListenerHelpers.GetExceptionArray($"EXCEPTION IN TRACER: UNABLE TO FORMAT {e}"));
        }
    }
}

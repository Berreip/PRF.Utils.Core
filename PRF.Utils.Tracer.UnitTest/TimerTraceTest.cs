﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.Tracer.Configuration;

namespace PRF.Utils.Tracer.UnitTest
{
    [TestClass]
    public class TimerTraceTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            foreach (TraceListener listener in Trace.Listeners)
            {
                // vérification pollution listener statique
                Assert.AreNotEqual(listener.Name, @"MainTracerSync", "il reste un traceur dans la liste des traceurs statiques = pollution");
            }
        }

        [TestMethod]
        public async Task TimerTraceTesttV1()
        {
            // setup
            int pagesReceived =0;
            // la date cible est maintenant + 1 seconde
            var timeTarget = DateTime.UtcNow.AddSeconds(1);

            // on décide d'envoyer une page toute les 200 ms, sur une seconde, on devrait avoir 5 pages environ
            var config = new TraceConfig
            {
                PageSize = 1000,
                MaximumTimeForFlush = TimeSpan.FromMilliseconds(200)
            };

            using (var traceListener = new TraceSourceSync(config))
            {
                // quand on reçoit une page, on incrémente le compteur
                traceListener.OnTracesSent += o => Interlocked.Increment(ref pagesReceived);

                // tant que la seconde n'est pas atteinte, on trace des messages (avec une attente)
                while (DateTime.UtcNow < timeTarget)
                {
                    Trace.TraceInformation("Method1");
                    await Task.Delay(50);
                }

                //Test
                await traceListener.FlushAndCompleteAddingAsync().ConfigureAwait(false);
            }

            //Verify que l'on a 6 pages ou moins (les 5 de flush + l'éventuelle dernière) PK moins? => car selon le Task.Delay, il se peut que l'on n'écrive aucune trace dans un cycle de 200ms et donc, que l'on envoie rien
            Assert.IsTrue(pagesReceived > 0 && pagesReceived <= 6, $"INVALID number of pages received = {pagesReceived}");
        }
    }

}

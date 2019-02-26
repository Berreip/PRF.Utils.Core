using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.Tracer.Listener;

namespace PRF.Utils.Tracer.UnitTest
{
    [TestClass]
    public class TraceListenerTest
    {
        /// <summary>
        /// Teste que le temps de flush doit être supérieur ou égal à 50 ms
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorV1()
        {
            //Configuration

            //Test
            using (var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(49), 1_000))
            {
            }

            //Verify
        }


        /// <summary>
        /// Teste que le buffer ne peut pas être négatif ou égal à zéro
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorV2()
        {
            //Configuration

            //Test
            using (var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(490), -1))
            {
            }

            //Verify
        }


        /// <summary>
        /// Teste que le dipose ne déclenche aucun pb
        /// </summary>
        [TestMethod]
        public void CtorV3()
        {
            //Configuration

            //Test
            using (var _ = new TraceListenerSync(TimeSpan.FromMilliseconds(490), 1_000))
            {
            }

            //Verify
        }



        /// <summary>
        /// Teste la limite de temps du listener (entre un minimum et un maximum)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorV4()
        {
            //Configuration

            //Test
            using (var _ = new TraceListenerSync(TimeSpan.FromHours(2), 1_000))
            {
            }

            //Verify
        }
    }
}

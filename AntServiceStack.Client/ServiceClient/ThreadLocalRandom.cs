using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AntServiceStack.Client.ServiceClient
{
    internal static class ThreadLocalRandom
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> Rnd =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        /// <summary>
        ///     The current random number seed available to this thread
        /// </summary>
        public static Random Current => Rnd.Value;
    }
}

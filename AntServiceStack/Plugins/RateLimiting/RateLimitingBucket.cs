using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Hystrix.Atomic;

namespace AntServiceStack.Plugins.RateLimiting
{
    internal class RateLimitingBucket
    {
        public long TimeInSeconds { get; private set; }
        protected ConcurrentDictionary<string, AtomicInteger> RequestCounters { get; set; }

        public RateLimitingBucket(long timeInSeconds)
        {
            TimeInSeconds = timeInSeconds;
            RequestCounters = new ConcurrentDictionary<string, AtomicInteger>();
        }

        public int this[string requestIdentity]
        {
            get
            {
                AtomicInteger counter;
                RequestCounters.TryGetValue(requestIdentity, out counter);
                return counter;
            }
        }

        public void IncreaseRequestCount(string requestIdentity)
        {
            AtomicInteger counter = RequestCounters.GetOrAdd(requestIdentity, id => new AtomicInteger());
            counter.IncrementAndGet();
        }

        public void DecreaseRequestCount(string requestIdentity)
        {
            AtomicInteger counter = RequestCounters.GetOrAdd(requestIdentity, id => new AtomicInteger());
            counter.DecrementAndGet();
        }
    }
}

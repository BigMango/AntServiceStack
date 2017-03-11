namespace CHystrix.Utils.Buffer
{
    using CHystrix.Utils.Atomic;
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class CounterBucket<T> : Bucket
    {
        public CounterBucket(long timeInMilliseconds) : base(timeInMilliseconds)
        {
            this.Counters = new ConcurrentDictionary<T, AtomicInteger>();
        }

        public void IncreaseCount(T identity)
        {
            this.Counters.GetOrAdd(identity, id => new AtomicInteger()).IncrementAndGet();
        }

        protected ConcurrentDictionary<T, AtomicInteger> Counters { get; set; }

        public int this[T identity]
        {
            get
            {
                AtomicInteger integer;
                this.Counters.TryGetValue(identity, out integer);
                return (int) integer;
            }
        }
    }
}


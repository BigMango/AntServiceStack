namespace CHystrix.Utils
{
    using CHystrix.Utils.Atomic;
    using System;
    using System.Runtime.CompilerServices;

    internal class IsolationSemaphore
    {
        private AtomicInteger UsedCount;

        public IsolationSemaphore(int count)
        {
            this.Count = count;
            this.UsedCount = new AtomicInteger();
        }

        public void Release()
        {
            this.UsedCount.DecrementAndGet();
        }

        public bool TryAcquire()
        {
            if (this.UsedCount.IncrementAndGet() > this.Count)
            {
                this.UsedCount.DecrementAndGet();
                return false;
            }
            return true;
        }

        public int Count { get; set; }

        public int CurrentCount
        {
            get
            {
                return (this.Count - this.UsedCount.Value);
            }
        }
    }
}


namespace CHystrix.Utils.Buffer
{
    using System;

    internal class CounterBuffer<T> : CircularBuffer<CounterBucket<T>>
    {
        public CounterBuffer(int timeWindowInMilliseconds, int bucketCount) : base((long) timeWindowInMilliseconds, bucketCount)
        {
        }

        protected override CounterBucket<T> CreateEmptyBucket(long timeInMilliseconds)
        {
            return new CounterBucket<T>(timeInMilliseconds);
        }

        public int GetCount(T identity)
        {
            long currentBucketStartTimeInMilliseconds = base.GetCurrentBucketStartTimeInMilliseconds();
            int num2 = 0;
            for (int i = 0; i < base.Buckets.Length; i++)
            {
                CounterBucket<T> bucket = base.Buckets[i];
                if ((currentBucketStartTimeInMilliseconds - bucket.TimeInMilliseconds) < base.TimeWindowInMilliseconds)
                {
                    num2 += bucket[identity];
                }
            }
            return num2;
        }

        public void IncreaseCount(T identity)
        {
            base.CurrentBucket.IncreaseCount(identity);
        }
    }
}


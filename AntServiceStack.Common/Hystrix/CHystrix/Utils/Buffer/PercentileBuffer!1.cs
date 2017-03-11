namespace CHystrix.Utils.Buffer
{
    using System;
    using System.Collections.Generic;

    internal class PercentileBuffer<T> : CircularBuffer<PercentileBucket<T>>
    {
        protected readonly int BucketCapacity;

        public PercentileBuffer(int timeWindowInMilliseconds, int bucketCount, int bucketCapacity) : base((long) timeWindowInMilliseconds, bucketCount)
        {
            if (bucketCapacity < 1)
            {
                throw new ArgumentException("Bucket capacity cannot be less than 1.");
            }
            this.BucketCapacity = bucketCapacity;
        }

        public void Add(T data)
        {
            base.CurrentBucket.Add(data);
        }

        protected override PercentileBucket<T> CreateEmptyBucket(long timeInMilliseconds)
        {
            return new PercentileBucket<T>(timeInMilliseconds, this.BucketCapacity);
        }

        public virtual List<T> GetSnapShot()
        {
            List<T> data = new List<T>();
            this.VisitData(delegate (T item) {
                data.Add(item);
            });
            return data;
        }

        public void VisitData(Action<T> consume)
        {
            long currentBucketStartTimeInMilliseconds = base.GetCurrentBucketStartTimeInMilliseconds();
            for (int i = 0; i < base.Buckets.Length; i++)
            {
                PercentileBucket<T> bucket = base.Buckets[i];
                if ((bucket.TimeInMilliseconds <= currentBucketStartTimeInMilliseconds) && ((bucket.TimeInMilliseconds + base.TimeWindowInMilliseconds) > currentBucketStartTimeInMilliseconds))
                {
                    int count = bucket.Count;
                    for (int j = 0; j < count; j++)
                    {
                        consume(bucket[j]);
                    }
                }
            }
        }
    }
}


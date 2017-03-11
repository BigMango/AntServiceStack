namespace CHystrix.Utils.Buffer
{
    using CHystrix.Utils;
    using System;
    using System.Threading;

    internal abstract class CircularBuffer<T> where T: Bucket
    {
        protected int _bufferEnd;
        protected readonly object AddBucketLock;
        protected readonly int BucketCount;
        protected readonly T[] Buckets;
        protected readonly long BucketTimeWindowInMilliseconds;
        protected const int LegacyBucketCount = 1;
        protected readonly long TimeWindowInMilliseconds;

        protected CircularBuffer(long timeWindowInMilliseconds, int bucketCount)
        {
            this.AddBucketLock = new object();
            if (timeWindowInMilliseconds < 1L)
            {
                throw new ArgumentException("Time window cannot be less than 1.");
            }
            if (bucketCount < 1)
            {
                throw new ArgumentException("Bucket count cannot be less than 1.");
            }
            if ((timeWindowInMilliseconds % ((long) bucketCount)) != 0L)
            {
                throw new ArgumentException("Time window must be n * bucket time window.");
            }
            this.TimeWindowInMilliseconds = timeWindowInMilliseconds;
            this.BucketCount = bucketCount;
            this.BucketTimeWindowInMilliseconds = timeWindowInMilliseconds / ((long) bucketCount);
            this.Buckets = new T[this.BucketCount + 1];
            for (int i = 0; i < this.Buckets.Length; i++)
            {
                this.Buckets[i] = this.CreateEmptyBucket(0L);
            }
        }

        protected abstract T CreateEmptyBucket(long timeInMilliseconds);
        protected long GetCurrentBucketStartTimeInMilliseconds()
        {
            long currentTimeInMiliseconds = CommonUtils.CurrentTimeInMiliseconds;
            return (currentTimeInMiliseconds - (currentTimeInMiliseconds % this.BucketTimeWindowInMilliseconds));
        }

        protected T CurrentBucket
        {
            get
            {
                long currentBucketStartTimeInMilliseconds = this.GetCurrentBucketStartTimeInMilliseconds();
                if (((this.Buckets[this._bufferEnd].TimeInMilliseconds + this.BucketTimeWindowInMilliseconds) <= currentBucketStartTimeInMilliseconds) && Monitor.TryEnter(this.AddBucketLock))
                {
                    try
                    {
                        if ((this.Buckets[this._bufferEnd].TimeInMilliseconds + this.BucketTimeWindowInMilliseconds) <= currentBucketStartTimeInMilliseconds)
                        {
                            int index = (this._bufferEnd + 1) % this.Buckets.Length;
                            this.Buckets[index] = this.CreateEmptyBucket(currentBucketStartTimeInMilliseconds);
                            this._bufferEnd = index;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this.AddBucketLock);
                    }
                }
                return this.Buckets[this._bufferEnd];
            }
        }
    }
}


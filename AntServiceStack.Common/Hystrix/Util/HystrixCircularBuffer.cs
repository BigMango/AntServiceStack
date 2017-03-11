using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using AntServiceStack.Common.Hystrix.Atomic;

namespace AntServiceStack.Common.Hystrix.Util
{
    public class HystrixCircularBuffer<T>
    {
        protected const int LegacyBucketCount = 2;

        protected readonly Bucket[] CircularBuffer;
        protected int _bufferEnd;
        protected readonly object AddBucketLock = new object();
        protected readonly int TimeWindowInSeconds;
        protected readonly int BucketTimeWindowInSeconds;
        protected readonly int BucketSizeLimit;
        protected readonly int BucketCount;

        public HystrixCircularBuffer(int timeWindowInSeconds, int bucketTimeWindowInSeconds, int bucketSizeLimit)
        {
            if (timeWindowInSeconds < 1)
                throw new ArgumentException("Time window cannot be less than 1.");
            if (bucketTimeWindowInSeconds < 1)
                throw new ArgumentException("Bucket time window cannot be less than 1.");
            if (bucketSizeLimit < 1)
                throw new ArgumentException("Bucket size limit cannot be less than 1.");

            BucketCount = timeWindowInSeconds / bucketTimeWindowInSeconds;
            if (BucketCount * bucketTimeWindowInSeconds != timeWindowInSeconds)
                throw new ArgumentException("Time window must be n * bucket time window.");

            TimeWindowInSeconds = timeWindowInSeconds;
            BucketTimeWindowInSeconds = bucketTimeWindowInSeconds;
            BucketSizeLimit = bucketSizeLimit;
            CircularBuffer = new Bucket[BucketCount + LegacyBucketCount];
            for (int i = 0; i < CircularBuffer.Length; i++)
                CircularBuffer[i] = new Bucket(0, BucketSizeLimit);
        }

        protected long GetCurrentTimeInSeconds()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        public void Add(T data)
        {
            long currentTimeInSeconds = GetCurrentTimeInSeconds();
            if (CircularBuffer[_bufferEnd].TimeInSeconds + BucketTimeWindowInSeconds <= currentTimeInSeconds)
            {
                if (Monitor.TryEnter(AddBucketLock))
                {
                    try
                    {
                        if (CircularBuffer[_bufferEnd].TimeInSeconds + BucketTimeWindowInSeconds <= currentTimeInSeconds)
                        {
                            int newBufferEnd = (_bufferEnd + 1) % CircularBuffer.Length;
                            CircularBuffer[newBufferEnd] = new Bucket(currentTimeInSeconds, BucketSizeLimit);
                            _bufferEnd = newBufferEnd;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(AddBucketLock);
                    }
                }
            }

            CircularBuffer[_bufferEnd].Add(data);
        }

        public void VisitData(Action<T> consume)
        {
            long currentTimeInSeconds = GetCurrentTimeInSeconds();
            long bucketTime = CircularBuffer[_bufferEnd].TimeInSeconds;
            if (currentTimeInSeconds >= bucketTime + TimeWindowInSeconds)
                return;
            while (bucketTime <= currentTimeInSeconds - BucketTimeWindowInSeconds * 2)
                bucketTime += BucketTimeWindowInSeconds;
            for (int i = 0; i < CircularBuffer.Length; i++)
            {
                Bucket bucket = CircularBuffer[i];
                if (bucket.TimeInSeconds <= bucketTime && bucket.TimeInSeconds + TimeWindowInSeconds > bucketTime)
                {
                    int count = bucket.Count;
                    for (int j = 0; j < count; j++)
                    {
                        consume(bucket[j]);
                    }
                }
            }
        }

        public virtual List<T> GetSnapShot()
        {
            List<T> data = new List<T>();
            VisitData(item => data.Add(item));
            return data;
        }

        protected class Bucket
        {
            public long TimeInSeconds { get; private set; }
            private T[] _data;
            private AtomicInteger _count;

            public Bucket(long timeInSeconds, int sizeLimit)
            {
                TimeInSeconds = timeInSeconds;
                _data = new T[sizeLimit];
                _count = new AtomicInteger();
            }

            public void Add(T data)
            {
                int index = (_count.IncrementAndGet() - 1) % _data.Length;
                _data[index] = data;
            }

            public int Count
            {
                get
                {
                    return Math.Min(_count.Value, _data.Length);
                }
            }

            public T this[int index]
            {
                get
                {
                    return _data[index];
                }
            }
        }
    }
}

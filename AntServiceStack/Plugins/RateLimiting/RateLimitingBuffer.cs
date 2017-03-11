using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AntServiceStack.Plugins.RateLimiting
{
    internal class RateLimitingBuffer
    {
        protected readonly int TimeSpan;
        protected readonly RateLimitingBucket[] CircularBuffer;
        protected readonly object AddBucketLock = new object();
        protected int _bufferEnd;

        public RateLimitingBuffer(int timeSpan)
        {
            TimeSpan = timeSpan;
            CircularBuffer = new RateLimitingBucket[TimeSpan + 1];
            for (int i = 0; i < CircularBuffer.Length; i++)
                CircularBuffer[i] = new RateLimitingBucket(0);
        }

        public int GetRate(string requestIdentity, long requestTimeInSeconds)
        {
            int rate = 0;
            for (int i = 0; i < CircularBuffer.Length; i++)
            {
                RateLimitingBucket bucket = CircularBuffer[i];
                if (requestTimeInSeconds - bucket.TimeInSeconds < TimeSpan)
                    rate += bucket[requestIdentity];
            }
            return rate;
        }

        public void AddRate(string requestIdentity, long requestTimeInSeconds)
        {
            if (CircularBuffer[_bufferEnd].TimeInSeconds < requestTimeInSeconds)
            {
                if (Monitor.TryEnter(AddBucketLock))
                {
                    try
                    {
                        if (CircularBuffer[_bufferEnd].TimeInSeconds < requestTimeInSeconds)
                        {
                            int newBufferEnd = (_bufferEnd + 1) % CircularBuffer.Length;
                            CircularBuffer[newBufferEnd] = new RateLimitingBucket(requestTimeInSeconds);
                            _bufferEnd = newBufferEnd;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(AddBucketLock);
                    }
                }
            }

            CircularBuffer[_bufferEnd].IncreaseRequestCount(requestIdentity);
        }

        public void ReduceRate(string requestIdentity, long requestTimeInSeconds)
        {
            for (int i = 0; i < CircularBuffer.Length; i++)
            {
                RateLimitingBucket bucket = CircularBuffer[i];
                if (bucket.TimeInSeconds == requestTimeInSeconds)
                {
                    bucket.DecreaseRequestCount(requestIdentity);
                    break;
                }
            }
        }
    }
}

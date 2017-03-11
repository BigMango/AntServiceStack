namespace CHystrix.Utils.Buffer
{
    using CHystrix.Utils.Atomic;
    using System;
    using System.Reflection;

    internal class PercentileBucket<T> : Bucket
    {
        private AtomicInteger _count;
        private T[] _data;

        public PercentileBucket(long timeInMilliseconds, int capacity) : base(timeInMilliseconds)
        {
            this._data = new T[capacity];
            this._count = new AtomicInteger();
        }

        public void Add(T data)
        {
            if (this._data.Length != 0)
            {
                int index = (this._count.IncrementAndGet() - 1) % this._data.Length;
                this._data[index] = data;
            }
        }

        public int Count
        {
            get
            {
                return Math.Min(this._count.Value, this._data.Length);
            }
        }

        public T this[int index]
        {
            get
            {
                return this._data[index];
            }
        }
    }
}


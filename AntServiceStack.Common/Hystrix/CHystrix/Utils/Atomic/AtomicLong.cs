namespace CHystrix.Utils.Atomic
{
    using System;
    using System.Globalization;
    using System.Threading;

    [Serializable]
    internal class AtomicLong : IFormattable
    {
        private long longValue;

        public AtomicLong() : this(0L)
        {
        }

        public AtomicLong(long initialValue)
        {
            this.longValue = initialValue;
        }

        public long AddAndGet(long delta)
        {
            return Interlocked.Add(ref this.longValue, delta);
        }

        public bool CompareAndSet(long expect, long update)
        {
            return (Interlocked.CompareExchange(ref this.longValue, update, expect) == expect);
        }

        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.longValue);
        }

        public override bool Equals(object obj)
        {
            return ((obj as AtomicLong) == this);
        }

        public long GetAndDecrement()
        {
            return (Interlocked.Decrement(ref this.longValue) + 1L);
        }

        public long GetAndIncrement()
        {
            return (Interlocked.Increment(ref this.longValue) - 1L);
        }

        public long GetAndSet(long newValue)
        {
            return Interlocked.Exchange(ref this.longValue, newValue);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref this.longValue);
        }

        public static bool operator ==(AtomicLong left, AtomicLong right)
        {
            return ((object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) || ((!object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) && (left.Value == right.Value)));
        }

        public static implicit operator long(AtomicLong atomic)
        {
            if (object.ReferenceEquals(atomic, null))
            {
                return 0L;
            }
            return atomic.Value;
        }

        public static bool operator !=(AtomicLong left, AtomicLong right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return this.ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return this.Value.ToString(formatProvider);
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.Value.ToString(formatProvider);
        }

        public bool WeakCompareAndSet(long expect, long update)
        {
            return this.CompareAndSet(expect, update);
        }

        public long Value
        {
            get
            {
                return Interlocked.Read(ref this.longValue);
            }
            set
            {
                Interlocked.Exchange(ref this.longValue, value);
            }
        }
    }
}


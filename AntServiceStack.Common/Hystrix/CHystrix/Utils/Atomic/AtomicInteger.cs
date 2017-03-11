namespace CHystrix.Utils.Atomic
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Serializable]
    internal class AtomicInteger : IFormattable
    {
        private volatile int integerValue;

        public AtomicInteger() : this(0)
        {
        }

        public AtomicInteger(int initialValue)
        {
            this.integerValue = initialValue;
        }

        public int AddAndGet(int delta)
        {
            return Interlocked.Add(ref this.integerValue, delta);
        }

        public bool CompareAndSet(int expect, int update)
        {
            return (Interlocked.CompareExchange(ref this.integerValue, update, expect) == expect);
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref this.integerValue);
        }

        public override bool Equals(object obj)
        {
            return ((obj as AtomicInteger) == this);
        }

        public int GetAndDecrement()
        {
            return (Interlocked.Decrement(ref this.integerValue) + 1);
        }

        public int GetAndIncrement()
        {
            return (Interlocked.Increment(ref this.integerValue) - 1);
        }

        public int GetAndSet(int newValue)
        {
            return Interlocked.Exchange(ref this.integerValue, newValue);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref this.integerValue);
        }

        public static bool operator ==(AtomicInteger left, AtomicInteger right)
        {
            return ((object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) || ((!object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) && (left.Value == right.Value)));
        }

        public static implicit operator int(AtomicInteger atomic)
        {
            if (object.ReferenceEquals(atomic, null))
            {
                return 0;
            }
            return atomic.Value;
        }

        public static bool operator !=(AtomicInteger left, AtomicInteger right)
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

        public bool WeakCompareAndSet(int expect, int update)
        {
            return this.CompareAndSet(expect, update);
        }

        public int Value
        {
            get
            {
                return this.integerValue;
            }
            set
            {
                this.integerValue = value;
            }
        }
    }
}


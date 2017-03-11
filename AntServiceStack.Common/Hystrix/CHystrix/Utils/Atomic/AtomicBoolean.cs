namespace CHystrix.Utils.Atomic
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Serializable]
    internal class AtomicBoolean : IFormattable
    {
        private volatile int booleanValue;

        public AtomicBoolean() : this(false)
        {
        }

        public AtomicBoolean(bool initialValue)
        {
            this.Value = initialValue;
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            int comparand = expect ? 1 : 0;
            int num2 = update ? 1 : 0;
            return (Interlocked.CompareExchange(ref this.booleanValue, num2, comparand) == comparand);
        }

        public override bool Equals(object obj)
        {
            return ((obj as AtomicBoolean) == this);
        }

        public bool GetAndSet(bool newValue)
        {
            return (Interlocked.Exchange(ref this.booleanValue, newValue ? 1 : 0) != 0);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(AtomicBoolean left, AtomicBoolean right)
        {
            return ((object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) || ((!object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) && (left.Value == right.Value)));
        }

        public static implicit operator bool(AtomicBoolean atomic)
        {
            if (object.ReferenceEquals(atomic, null))
            {
                return false;
            }
            return atomic.Value;
        }

        public static bool operator !=(AtomicBoolean left, AtomicBoolean right)
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

        public bool WeakCompareAndSet(bool expect, bool update)
        {
            return this.CompareAndSet(expect, update);
        }

        public bool Value
        {
            get
            {
                return (this.booleanValue != 0);
            }
            set
            {
                this.booleanValue = value ? 1 : 0;
            }
        }
    }
}


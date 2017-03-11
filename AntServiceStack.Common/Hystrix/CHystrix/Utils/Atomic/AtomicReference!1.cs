namespace CHystrix.Utils.Atomic
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Serializable]
    internal class AtomicReference<T> where T: class
    {
        private volatile T reference;

        public AtomicReference() : this(default(T))
        {
        }

        public AtomicReference(T initialValue)
        {
            this.reference = initialValue;
        }

        public bool CompareAndSet(T expect, T update)
        {
            return object.ReferenceEquals(expect, Interlocked.CompareExchange<T>(ref this.reference, update, expect));
        }

        public override bool Equals(object obj)
        {
            return ((obj as AtomicReference<T>) == this);
        }

        public T GetAndSet(T newValue)
        {
            return Interlocked.Exchange<T>(ref this.reference, newValue);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(AtomicReference<T> left, AtomicReference<T> right)
        {
            return ((!object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) && object.ReferenceEquals(left.Value, right.Value));
        }

        public static implicit operator T(AtomicReference<T> atomic)
        {
            if (atomic == null)
            {
                return default(T);
            }
            return atomic.Value;
        }

        public static bool operator !=(AtomicReference<T> left, AtomicReference<T> right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public bool WeakCompareAndSet(T expect, T update)
        {
            return this.CompareAndSet(expect, update);
        }

        public T Value
        {
            get
            {
                return this.reference;
            }
            set
            {
                this.reference = value;
            }
        }
    }
}


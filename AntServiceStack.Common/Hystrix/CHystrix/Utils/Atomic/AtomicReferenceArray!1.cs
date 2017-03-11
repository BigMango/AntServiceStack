namespace CHystrix.Utils.Atomic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [Serializable]
    internal class AtomicReferenceArray<T>
    {
        private readonly T[] array;

        public AtomicReferenceArray(int length)
        {
            this.array = new T[length];
        }

        public AtomicReferenceArray(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this.array = new T[array.Length];
            Array.Copy(array, 0, this.array, 0, array.Length);
        }

        public AtomicReferenceArray(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            this.array = items.ToArray<T>();
        }

        public bool CompareAndSet(int index, T expect, T update)
        {
            lock (((AtomicReferenceArray<T>) this))
            {
                if (this.array[index].Equals(expect))
                {
                    this.array[index] = update;
                    return true;
                }
                return false;
            }
        }

        public T GetAndSet(int index, T newValue)
        {
            lock (this.array)
            {
                T local = this.array[index];
                this.array[index] = newValue;
                return local;
            }
        }

        public static implicit operator T[](AtomicReferenceArray<T> atomic)
        {
            if (atomic == null)
            {
                return null;
            }
            return atomic.ToArray();
        }

        public T[] ToArray()
        {
            return (T[]) this.array.Clone();
        }

        public bool WeakCompareAndSet(int index, T expect, T update)
        {
            return this.CompareAndSet(index, expect, update);
        }

        public T this[int index]
        {
            get
            {
                lock (this.array)
                {
                    return this.array[index];
                }
            }
            set
            {
                lock (this.array)
                {
                    this.array[index] = value;
                }
            }
        }

        public int Length
        {
            get
            {
                return this.array.Length;
            }
        }
    }
}


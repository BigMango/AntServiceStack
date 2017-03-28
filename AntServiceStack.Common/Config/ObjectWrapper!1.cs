namespace AntServiceStack.Common.Config
{
    using System;
    using System.Runtime.CompilerServices;

    public class ObjectWrapper<T>
    {
        public ObjectWrapper()
        {
        }

        public ObjectWrapper(T value)
        {
            this.Value = value;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is T)
            {
                return object.Equals(this.Value, (T) obj);
            }
            ObjectWrapper<T> wrapper = obj as ObjectWrapper<T>;
            if (wrapper == null)
            {
                return false;
            }
            return object.Equals(this.Value, wrapper.Value);
        }

        public override int GetHashCode()
        {
            if (this.Value != null)
            {
                return this.Value.GetHashCode();
            }
            return 0;
        }

        public static implicit operator T(ObjectWrapper<T> value)
        {
            if (value == null)
            {
                return default(T);
            }
            return value.Value;
        }

        public static implicit operator ObjectWrapper<T>(T value)
        {
            return new ObjectWrapper<T>(value);
        }

        public override string ToString()
        {
            if (this.Value != null)
            {
                return this.Value.ToString();
            }
            return "";
        }

        public T Value { get; set; }
    }
}


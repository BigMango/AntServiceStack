using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public interface IProperty
    {
        string Key { get; }

        string Value { get; }

        PropertyConfig Config { get; }

        event EventHandler<PropertyChangedEventArgs> OnChange;

        void Refresh();
    }
    public interface IProperty<T> : IProperty
    {
        T Value { get; }

        PropertyConfig<T> Config { get; }

        event EventHandler<PropertyChangedEventArgs<T>> OnChange;
    }
    public interface IValueParser<T>
    {
        T Parse(string input);

        bool TryParse(string input, out T result);
    }
    public interface IValueCorrector<T>
    {
        T Correct(T value);
    }
    public class PropertyConfig<T> : PropertyConfig
    {
        public T DefaultValue { get; private set; }

        public IValueParser<T> ValueParser { get; internal set; }

        public IValueCorrector<T> ValueCorrector { get; private set; }

        public PropertyConfig()
          : this(default(T), true, (IValueParser<T>)null, (IValueCorrector<T>)null)
        {
        }

        public PropertyConfig(T defaultValue)
          : this(defaultValue, true, (IValueParser<T>)null, (IValueCorrector<T>)null)
        {
        }

        public PropertyConfig(T defaultValue, bool useCache)
          : this(defaultValue, useCache, (IValueParser<T>)null, (IValueCorrector<T>)null)
        {
        }

        public PropertyConfig(T defaultValue, IValueParser<T> valueParser)
          : this(defaultValue, true, valueParser, (IValueCorrector<T>)null)
        {
        }

        public PropertyConfig(T defaultValue, IValueCorrector<T> valueCorrector)
          : this(defaultValue, true, (IValueParser<T>)null, valueCorrector)
        {
        }

        public PropertyConfig(T defaultValue, bool useCache, IValueParser<T> valueParser)
          : this(defaultValue, useCache, valueParser, (IValueCorrector<T>)null)
        {
        }

        public PropertyConfig(T defaultValue, bool useCache, IValueCorrector<T> valueCorrector)
          : this(defaultValue, useCache, (IValueParser<T>)null, valueCorrector)
        {
        }

        public PropertyConfig(T defaultValue, IValueParser<T> valueParser, IValueCorrector<T> valueCorrector)
          : this(defaultValue, true, valueParser, valueCorrector)
        {
        }

        public PropertyConfig(T defaultValue, bool useCache, IValueParser<T> valueParser, IValueCorrector<T> valueCorrector)
          : base((object)defaultValue == null ? (string)null : defaultValue.ToString(), useCache)
        {
            this.DefaultValue = defaultValue;
            this.ValueParser = valueParser;
            this.ValueCorrector = valueCorrector;
        }

        public override string ToString()
        {
            return string.Format("{{\"DefaultValue\":\"{0}\",\"UseCache\":\"{1}\"}}", (object)this.DefaultValue, (object)this.UseCache);
        }
    }

    public class RangeCorrector<T> : ValueCorrectorBase<T> where T : IComparable<T>
    {
        public T Min { get; private set; }

        public T Max { get; private set; }

        public RangeCorrector(T min, T max)
          : this((IValueCorrector<T>)null, min, max)
        {
        }

        public RangeCorrector(IValueCorrector<T> innerValueCorrector, T min, T max)
          : base(innerValueCorrector)
        {
            this.Min = min;
            this.Max = max;
        }

        protected override T CorrectValue(T value)
        {
            if ((object)value == null)
                return value;
            if ((object)this.Min != null && value.CompareTo(this.Min) < 0)
                value = this.Min;
            if ((object)this.Max != null && value.CompareTo(this.Max) > 0)
                value = this.Max;
            return value;
        }
    }

    public abstract class ValueCorrectorBase<T> : IValueCorrector<T>
    {
        private IValueCorrector<T> innerValueCorrector;

        public ValueCorrectorBase(IValueCorrector<T> innerValueCorrector)
        {
            this.innerValueCorrector = innerValueCorrector;
        }

        public T Correct(T value)
        {
            if (this.innerValueCorrector != null)
                value = this.innerValueCorrector.Correct(value);
            return this.CorrectValue(value);
        }

        protected abstract T CorrectValue(T value);
    }
}

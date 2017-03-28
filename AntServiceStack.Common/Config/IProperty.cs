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
}

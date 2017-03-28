using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Config.Property;
using AntServiceStack.Common.Config.ValueParser;
using AntServiceStack.Common.Extensions;
using Freeway.Logging;

namespace AntServiceStack.Common.Config
{
    internal class DefaultConfigurationManager : IConfigurationManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DefaultConfigurationManager));

        protected List<IConfigurationSource> ConfigurationSources { get; private set; }

        protected ConcurrentDictionary<string, IProperty> PropertyCache { get; private set; }

        protected ConcurrentDictionary<Type, object> ValueParserCache { get; private set; }

        public IEnumerable<IConfigurationSource> Sources
        {
            get
            {
                return (IEnumerable<IConfigurationSource>)this.ConfigurationSources;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return (IEnumerable<string>)this.PropertyCache.Keys;
            }
        }

        public event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        public DefaultConfigurationManager(IEnumerable<IConfigurationSource> sources)
        {
            this.PropertyCache = new ConcurrentDictionary<string, IProperty>();
            this.ValueParserCache = new ConcurrentDictionary<Type, object>();
            this.ConfigurationSources = new List<IConfigurationSource>();
            this.RegisterBuildInValueParser();
            if (sources == null)
                return;
            foreach (IConfigurationSource source in sources)
            {
                if (source != null && source.Configuration != null)
                    this.ConfigurationSources.Add(source);
            }
            this.ConfigurationSources.Sort((IComparer<IConfigurationSource>)new ConfigurationSourceComparer());
            this.ConfigurationSources.OfType<IDynamicConfigurationSource>().Foreach<IDynamicConfigurationSource>((Action<IDynamicConfigurationSource>)(source => source.OnChange += new EventHandler<ConfigurationChangedEventArgs>(this.OnConfigurationSourceChanged)));
        }

        private void RegisterBuildInValueParser()
        {
            this.Register<bool>((IValueParser<bool>)BoolParser.Instance);
            this.RegisterNullable<bool>((IValueParser<bool>)BoolParser.Instance);
            this.Register<char>((IValueParser<char>)CharParser.Instance);
            this.RegisterNullable<char>((IValueParser<char>)CharParser.Instance);
            this.Register<byte>((IValueParser<byte>)ByteParser.Instance);
            this.RegisterNullable<byte>((IValueParser<byte>)ByteParser.Instance);
            this.Register<sbyte>((IValueParser<sbyte>)SByteParser.Instance);
            this.RegisterNullable<sbyte>((IValueParser<sbyte>)SByteParser.Instance);
            this.Register<short>((IValueParser<short>)ShortParser.Instance);
            this.RegisterNullable<short>((IValueParser<short>)ShortParser.Instance);
            this.Register<ushort>((IValueParser<ushort>)UShortParser.Instance);
            this.RegisterNullable<ushort>((IValueParser<ushort>)UShortParser.Instance);
            this.Register<int>((IValueParser<int>)IntParser.Instance);
            this.RegisterNullable<int>((IValueParser<int>)IntParser.Instance);
            this.Register<uint>((IValueParser<uint>)UIntParser.Instance);
            this.RegisterNullable<uint>((IValueParser<uint>)UIntParser.Instance);
            this.Register<long>((IValueParser<long>)LongParser.Instance);
            this.RegisterNullable<long>((IValueParser<long>)LongParser.Instance);
            this.Register<ulong>((IValueParser<ulong>)ULongParser.Instance);
            this.RegisterNullable<ulong>((IValueParser<ulong>)ULongParser.Instance);
            this.Register<float>((IValueParser<float>)FloatParser.Instance);
            this.RegisterNullable<float>((IValueParser<float>)FloatParser.Instance);
            this.Register<double>((IValueParser<double>)DoubleParser.Instance);
            this.RegisterNullable<double>((IValueParser<double>)DoubleParser.Instance);
            this.Register<Decimal>((IValueParser<Decimal>)DecimalParser.Instance);
            this.RegisterNullable<Decimal>((IValueParser<Decimal>)DecimalParser.Instance);
            this.Register<DateTime>((IValueParser<DateTime>)DateTimeParser.Instance);
            this.RegisterNullable<DateTime>((IValueParser<DateTime>)DateTimeParser.Instance);
            this.Register<Guid>((IValueParser<Guid>)GuidParser.Instance);
            this.RegisterNullable<Guid>((IValueParser<Guid>)GuidParser.Instance);
            this.Register<Version>((IValueParser<Version>)VersionParser.Instance);
            this.Register<string>((IValueParser<string>)StringParser.Instance);
        }

        protected void OnConfigurationSourceChanged(object sender, ConfigurationChangedEventArgs e)
        {
            ((IEnumerable<PropertyChangedEventArgs>)e.ChangedProperties).Foreach<PropertyChangedEventArgs>((Action<PropertyChangedEventArgs>)(item =>
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    return;
                IProperty property;
                if (this.PropertyCache.TryGetValue(item.Key, out property))
                    property.Refresh();
                if (this.OnPropertyChange == null || string.Equals(item.OldValue, item.NewValue))
                    return;
                this.OnPropertyChange((object)this, new PropertyChangedEventArgs(item.Key, item.OldValue, item.NewValue, item.ChangedTime));
            }));
        }

        public string GetPropertyValue(string key, string defaultValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            foreach (IConfigurationSource configurationSource in this.ConfigurationSources)
            {
                string propertyValue = configurationSource.Configuration.GetPropertyValue(key);
                if (propertyValue != null)
                {
                    DefaultConfigurationManager.Logger.Info(string.Format("The {0}={1} has been found in source: {2}.", (object)key, (object)propertyValue, (object)configurationSource.SourceId));
                    return string.IsNullOrWhiteSpace(propertyValue) ? (string)null : propertyValue;
                }
            }
            return defaultValue;
        }

        protected IProperty NewProperty(string key, PropertyConfig config)
        {
            return (IProperty)new DefaultProperty((IConfigurationManager)this, key, config);
        }

        protected IProperty<T> NewProperty<T>(string key, PropertyConfig<T> config)
        {
            if (config.ValueParser == null)
                config.ValueParser = this.GetValueParser<T>();
            return (IProperty<T>)new DefaultProperty<T>((IConfigurationManager)this, key, config);
        }

        public IProperty GetProperty(string key, PropertyConfig config)
        {
            key = key.NotEmptyOrWhiteSpace("key");
            return this.PropertyCache.GetOrAdd(key, this.NewProperty(key, config));
        }

        public IProperty<T> GetProperty<T>(string key, PropertyConfig<T> config)
        {
            key = key.NotEmptyOrWhiteSpace("key");
            return (IProperty<T>)this.PropertyCache.GetOrAdd(key, (IProperty)this.NewProperty<T>(key, config));
        }

        public void Register<T>(IValueParser<T> valueParser)
        {
            valueParser = valueParser.NotNull("valueParser");
            this.ValueParserCache[typeof(T)] = (object)valueParser;
        }

        private void RegisterNullable<T>(IValueParser<T> valueParser) where T : struct
        {
            this.Register<T?>((IValueParser<T?>)new NullableParser<T>(valueParser));
        }

        private IValueParser<T> GetValueParser<T>()
        {
            object obj;
            if (this.ValueParserCache.TryGetValue(typeof(T), out obj))
                return (IValueParser<T>)obj;
            return (IValueParser<T>)null;
        }
    }


    internal class ConfigurationSourceComparer : IComparer<IConfigurationSource>
    {
        public int Compare(IConfigurationSource o1, IConfigurationSource o2)
        {
            if (o1 == o2)
                return 0;
            if (o1 == null)
                return 1;
            if (o2 == null)
                return -1;
            if (o1.Priority == o2.Priority)
                return 0;
            return o1.Priority >= o2.Priority ? -1 : 1;
        }
    }
}

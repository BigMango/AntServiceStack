using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public class MemoryConfiguration : IMemoryConfiguration, IDynamicConfiguration, IConfiguration
    {
        protected ConcurrentDictionary<string, string> ConfigurationMap { get; set; }

        public virtual string this[string key]
        {
            get
            {
                return this.GetPropertyValue(key);
            }
            set
            {
                this.SetPropertyValue(key, value);
            }
        }

        public virtual event EventHandler<ConfigurationChangedEventArgs> OnChange;

        public MemoryConfiguration()
        {
            this.ConfigurationMap = new ConcurrentDictionary<string, string>();
        }

        public MemoryConfiguration(IEnumerable<KeyValuePair<string, string>> collection)
        {
            this.ConfigurationMap = new ConcurrentDictionary<string, string>();
            foreach (KeyValuePair<string, string> keyValuePair in collection)
            {
                if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                    throw new ArgumentException("Configuration key can not be null or empty.");
                this.ConfigurationMap.TryAdd(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public virtual string SetPropertyValue(string key, string value)
        {
            string oldValue = (string)null;
            this.ConfigurationMap.AddOrUpdate(key, value, (Func<string, string, string>)((k, v) =>
            {
                oldValue = v;
                return value;
            }));
            if (this.OnChange != null && !object.Equals((object)oldValue, (object)value))
                this.OnChange((object)this, new ConfigurationChangedEventArgs((IEnumerable<PropertyChangedEventArgs>)new PropertyChangedEventArgs[1]
                {
          new PropertyChangedEventArgs(key, oldValue, value)
                }));
            return oldValue;
        }

        public virtual string GetPropertyValue(string key)
        {
            if (!this.ConfigurationMap.ContainsKey(key))
                return (string)null;
            string str = this.ConfigurationMap[key];
            if (!string.IsNullOrWhiteSpace(str))
                return str;
            return (string)null;
        }
    }
}

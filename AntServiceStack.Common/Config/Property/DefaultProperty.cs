using AntServiceStack.Common.Extensions;
using Freeway.Logging;

namespace AntServiceStack.Common.Config.Property
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class DefaultProperty : IProperty
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DefaultProperty));
        private volatile string value;

        public event EventHandler<PropertyChangedEventArgs> OnChange;

        public DefaultProperty(IConfigurationManager manager, string key, PropertyConfig config)
        {
            manager = manager.NotNull("manager");
            key= key.NotEmptyOrWhiteSpace("key");
            config = config.NotNull("config");
            this.Manager = manager;
            this.Key = key;
            this.Config = config;
            this.Refresh();
        }

        private void RaisePropertyChangeEvent(string oldValue, string newValue)
        {
            if (this.OnChange != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(this.Key, oldValue, newValue);
                try
                {
                    this.OnChange(this, e);
                }
                catch (Exception exception)
                {
                    Logger.Error("Error occurred while raising property change event", exception);
                }
            }
        }

        public virtual void Refresh()
        {
            string propertyValue = this.Manager.GetPropertyValue(this.Key, null);
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                propertyValue = this.Config.DefaultValue;
            }
            string a = this.value;
            if (!string.Equals(a, propertyValue))
            {
                this.value = propertyValue;
                this.RaisePropertyChangeEvent(a, propertyValue);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{");
            builder.AppendFormat("\"Key\":\"{0}\",", this.Key);
            builder.AppendFormat("\"Value\":\"{0}\",", this.Value);
            builder.AppendFormat("\"Config\":\"{0}\"", this.Config.ToString());
            builder.Append('}');
            return builder.ToString();
        }

        public PropertyConfig Config { get; private set; }

        public string Key { get; private set; }

        protected IConfigurationManager Manager { get; private set; }

        public string Value
        {
            get
            {
                if (!this.Config.UseCache)
                {
                    this.Refresh();
                }
                return this.value;
            }
        }
    }
}


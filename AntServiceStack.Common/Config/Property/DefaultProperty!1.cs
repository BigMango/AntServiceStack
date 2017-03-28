using AntServiceStack.Common.Config;
using Freeway.Logging;

namespace AntServiceStack.Common.Config.Property
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class DefaultProperty<T> : DefaultProperty, IProperty<T>, IProperty
    {
        private static ILog Logger;
        private volatile ObjectWrapper<T> value;

        public event EventHandler<PropertyChangedEventArgs<T>> OnChange;

        static DefaultProperty()
        {
            DefaultProperty<T>.Logger = LogManager.GetLogger(typeof(DefaultProperty<>));
        }

        public DefaultProperty(IConfigurationManager manager, string key, PropertyConfig<T> config) : base(manager, key, config)
        {
        }

        private void RaisePropertyChangeEvent(T oldValue, T newValue)
        {
            if (this.OnChange != null)
            {
                PropertyChangedEventArgs<T> e = new PropertyChangedEventArgs<T>(base.Key, oldValue, newValue);
                try
                {
                    this.OnChange(this, e);
                }
                catch (Exception exception)
                {
                    DefaultProperty<T>.Logger.Error("Error occurred while raising property change event", exception);
                }
            }
        }

        public override void Refresh()
        {
            T defaultValue;
            string propertyValue = base.Manager.GetPropertyValue(base.Key, null);
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                defaultValue = this.Config.DefaultValue;
            }
            else if (this.Config.ValueParser == null)
            {
                defaultValue = this.Config.DefaultValue;
                DefaultProperty<T>.Logger.Warn(string.Format("ValueParser is null! Key:{0}, Value type:{1}", base.Key, typeof(T).FullName));
            }
            else if (!this.Config.ValueParser.TryParse(propertyValue, out defaultValue))
            {
                defaultValue = this.Config.DefaultValue;
                DefaultProperty<T>.Logger.Warn(string.Format("Failed to parse property value! Key:{0}, String value:{1}, ValueParser:{2}", base.Key, propertyValue, this.Config.ValueParser.GetType().FullName));
            }
            if (this.Config.ValueCorrector != null)
            {
                defaultValue = this.Config.ValueCorrector.Correct(defaultValue);
            }
            ObjectWrapper<T> objA = this.value;
            if (!object.Equals(objA, defaultValue))
            {
                this.value = defaultValue;
                this.RaisePropertyChangeEvent((T) objA, defaultValue);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{");
            builder.AppendFormat("\"Key\":\"{0}\",", base.Key);
            builder.AppendFormat("\"Value\":\"{0}\",", this.Value);
            builder.AppendFormat("\"Config\":\"{0}\"", this.Config.ToString());
            builder.Append('}');
            return builder.ToString();
        }

        public PropertyConfig<T> Config
        {
            get
            {
                return (PropertyConfig<T>) base.Config;
            }
        }

        public T Value
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


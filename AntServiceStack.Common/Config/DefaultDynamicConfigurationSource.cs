using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public class DefaultDynamicConfigurationSource : DefaultConfigurationSource, IDynamicConfigurationSource, IConfigurationSource
    {
        protected IDynamicConfiguration DynamicConfiguration
        {
            get
            {
                return (IDynamicConfiguration)this.Configuration;
            }
            set
            {
                this.Configuration = (IConfiguration)value;
            }
        }

        public event EventHandler<ConfigurationChangedEventArgs> OnChange
        {
            add
            {
                this.DynamicConfiguration.OnChange += value;
            }
            remove
            {
                this.DynamicConfiguration.OnChange -= value;
            }
        }

        public DefaultDynamicConfigurationSource(int priority, string sourceId, IDynamicConfiguration configuration)
          : base(priority, sourceId, (IConfiguration)configuration)
        {
        }
    }
}

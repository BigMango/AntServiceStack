using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public interface IDynamicConfigurationSource : IConfigurationSource
    {
        event EventHandler<ConfigurationChangedEventArgs> OnChange;
    }
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public PropertyChangedEventArgs[] ChangedProperties { get; private set; }

        public ConfigurationChangedEventArgs(IEnumerable<PropertyChangedEventArgs> changedProperties)
        {
            this.ChangedProperties = changedProperties.ToArray<PropertyChangedEventArgs>();
        }
    }
}

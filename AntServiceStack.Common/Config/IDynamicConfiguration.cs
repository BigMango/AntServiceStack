using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public interface IDynamicConfiguration : IConfiguration
    {
        event EventHandler<ConfigurationChangedEventArgs> OnChange;
    }
}

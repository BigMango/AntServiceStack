using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public interface IMemoryConfiguration : IDynamicConfiguration, IConfiguration
    {
        string this[string key] { get; set; }

        string SetPropertyValue(string key, string value);
    }
}

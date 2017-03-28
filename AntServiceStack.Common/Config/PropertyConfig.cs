using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public class PropertyConfig
    {
        public string DefaultValue { get; private set; }

        public bool UseCache { get; private set; }

        public PropertyConfig()
          : this((string)null, true)
        {
        }

        public PropertyConfig(string defaultValue)
          : this(defaultValue, true)
        {
        }

        public PropertyConfig(string defaultValue, bool useCache)
        {
            this.UseCache = useCache;
            this.DefaultValue = defaultValue;
        }

        public override string ToString()
        {
            return string.Format("{{\"DefaultValue\":\"{0}\",\"UseCache\":\"{1}\"}}", (object)this.DefaultValue, (object)this.UseCache);
        }
    }
}

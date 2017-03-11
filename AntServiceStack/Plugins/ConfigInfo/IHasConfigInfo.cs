using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Plugins.ConfigInfo
{
    public interface IHasConfigInfo
    {
        IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath);
    }
}

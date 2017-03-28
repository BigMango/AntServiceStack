using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Config;

namespace AntServiceStack.Common.Config
{
    public interface IConfigurationSource
    {
        int Priority { get; }

        string SourceId { get; }

        IConfiguration Configuration { get; }
    }
}

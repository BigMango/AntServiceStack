using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public class DefaultConfigurationSource : IConfigurationSource
    {
        public int Priority { get; protected set; }

        public string SourceId { get; protected set; }

        public IConfiguration Configuration { get; protected set; }

        public DefaultConfigurationSource(int priority, string sourceId, IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("Argument \"sourceId\" is null or empty");
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            this.Priority = priority;
            this.SourceId = sourceId;
            this.Configuration = configuration;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.RateLimiting
{
    public class ServiceRateLimitingPlugin : RateLimitingPlugin
    {
        protected const string EnableServiceRateLimitingCheckSettingKey = "SOA.EnableServiceRateLimitingCheck";
        protected const string ServiceRateLimitSettingKey = "SOA.ServiceRateLimit";

        protected override int DefaultRateLimit
        {
            get
            {
                return 10000;
            }
        }

        protected override string EnableRateLimitingCheckSettingKey
        {
            get { return EnableServiceRateLimitingCheckSettingKey; }
        }

        protected override string RateLimitSettingKey
        {
            get { return ServiceRateLimitSettingKey; }
        }

        protected override string GenerateRequestIdentity(IHttpRequest request)
        {
            return EndpointHost.Config.MetadataMap[request.ServicePath].FullServiceName;
        }

        protected override string Name
        {
            get
            {
                return "Service Rate Limiting";
            }
        }
    }
}

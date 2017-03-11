using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.RateLimiting
{
    public class IPRateLimitingPlugin : RateLimitingWithMapPlugin
    {
        protected const string EnableIPRateLimitingCheckSettingKey = "SOA.EnableIPRateLimitingCheck";
        protected const string IPRateLimitSettingKey = "SOA.IPRateLimit";
        protected const string IPRateLimitMapSettingKey = "SOA.IPRateLimitMap";

        protected override string EnableRateLimitingCheckSettingKey
        {
            get { return EnableIPRateLimitingCheckSettingKey; }
        }

        protected override string RateLimitSettingKey
        {
            get { return IPRateLimitSettingKey; }
        }

        protected override string RateLimitMapSettingKey
        {
            get { return IPRateLimitMapSettingKey; }
        }

        protected override string GenerateRequestIdentity(IHttpRequest request)
        {
            return request.RemoteIp ?? string.Empty;
        }

        protected override string Name
        {
            get
            {
                return "IP Rate Limiting";
            }
        }
    }
}

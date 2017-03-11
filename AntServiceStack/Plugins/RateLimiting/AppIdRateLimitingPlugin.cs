using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.Plugins.RateLimiting
{
    public class AppIdRateLimitingPlugin : RateLimitingWithMapPlugin
    {
        protected const string EnableAppIdRateLimitingCheckSettingKey = "SOA.EnableAppIdRateLimitingCheck";
        protected const string AppIdRateLimitSettingKey = "SOA.AppIdRateLimit";
        protected const string AppIdRateLimitMapSettingKey = "SOA.AppIdRateLimitMap";

        protected override string EnableRateLimitingCheckSettingKey
        {
            get { return EnableAppIdRateLimitingCheckSettingKey; }
        }

        protected override string RateLimitSettingKey
        {
            get { return AppIdRateLimitSettingKey; }
        }

        protected override string RateLimitMapSettingKey
        {
            get { return AppIdRateLimitMapSettingKey; }
        }

        protected override string GenerateRequestIdentity(IHttpRequest request)
        {
            string appId = request.Headers[ServiceUtils.AppIdHttpHeaderKey];
            if (string.IsNullOrWhiteSpace(appId))
                appId = string.Empty;
            return appId;
        }

        protected override string Name
        {
            get
            {
                return "AppId Rate Limiting";
            }
        }
    }
}

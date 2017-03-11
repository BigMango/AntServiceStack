using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using AntServiceStack.Text;
using AntServiceStack.Common;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Configuration;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.RateLimiting
{
    public class OperationRateLimitingPlugin : RateLimitingPlugin
    {
        protected const string EnableOperationRateLimitingCheckSettingKey = "SOA.EnableOperationRateLimitingCheck";
        protected const string OperationRateLimitSettingKey = "SOA.OperationRateLimit";
        protected const string OperationRateLimitMapSettingKey = "SOA.OperationRateLimitMap";

        protected static string OperationName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(HostContext.Instance.Request.OperationName))
                    return string.Empty;
                return HostContext.Instance.Request.OperationName.Trim().ToLower();
            }
        }

        protected readonly Dictionary<string, Dictionary<string, int>> RateLimitSettingMaps;

        public OperationRateLimitingPlugin()
        {
            RateLimitSettingMaps = new Dictionary<string, Dictionary<string, int>>();
        }

        protected override void Init()
        {
            base.Init();

            Dictionary<string, int> defaultRateLimitSettingMap = GetRateLimitSettingMap(OperationRateLimitMapSettingKey);
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string operationRateLimitMapSettingKey = metadata.GetServiceSpecificSettingKey(OperationRateLimitMapSettingKey);
                RateLimitSettingMaps[metadata.ServicePath] = GetRateLimitSettingMap(operationRateLimitMapSettingKey);
                foreach (KeyValuePair<string, int> pair in defaultRateLimitSettingMap)
                {
                    if (!RateLimitSettingMaps[metadata.ServicePath].ContainsKey(pair.Key))
                        RateLimitSettingMaps[metadata.ServicePath][pair.Key] = pair.Value;
                }
            }
        }

        protected Dictionary<string, int> GetRateLimitSettingMap(string settingKey)
        {
            Dictionary<string, int> rateLimitSettingMap = new Dictionary<string, int>();

            Dictionary<string, string> settingValues = ConfigUtils.GetDictionaryFromAppSettingValue(ConfigurationManager.AppSettings[settingKey]);
            foreach (KeyValuePair<string, string> pair in settingValues)
            {
                int rate;
                int.TryParse(pair.Value.Trim(), out rate);
                if (rate <= 0)
                    continue;
                rateLimitSettingMap[pair.Key.Trim().ToLower()] = rate;
            }

            return rateLimitSettingMap;
        }

        protected override int RateLimit
        {
            get
            {
                return RateLimitSettingMaps[ServicePath][OperationName];
            }
        }

        protected override bool Enabled
        {
            get
            {
                return base.Enabled && RateLimitSettingMaps[ServicePath].ContainsKey(OperationName) && RateLimitSettingMaps[ServicePath][OperationName] > 0;
            }
        }

        protected override string EnableRateLimitingCheckSettingKey
        {
            get { return EnableOperationRateLimitingCheckSettingKey; }
        }

        protected override string RateLimitSettingKey
        {
            get { return OperationRateLimitSettingKey; }
        }

        protected override string GenerateRequestIdentity(IHttpRequest request)
        {
            return EndpointHost.Config.MetadataMap[ServicePath].FullServiceName + "." + OperationName;
        }

        public override void Refresh(string servicePath, bool? enabled = null, int? rateLimit = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
                {
                    { EnableOperationRateLimitingCheckSettingKey, RateLimitingSettings[servicePath].Enabled },
                    { OperationRateLimitMapSettingKey, RateLimitSettingMaps[servicePath] }
                };
        }

        public virtual void Refresh(string servicePath, bool? enabled, Dictionary<string, int> rateLimitSettingMap = null)
        {
            if (enabled.HasValue)
                RateLimitingSettings[servicePath].Enabled = enabled.Value;

            if (rateLimitSettingMap != null)
                RateLimitSettingMaps[servicePath] = rateLimitSettingMap;
        }

        internal Dictionary<string, int> GetRateLimitMap(string servicePath)
        {
            return RateLimitSettingMaps[servicePath];
        }

        protected override string Name
        {
            get
            {
                return "Operation Rate Limiting";
            }
        }
    }
}

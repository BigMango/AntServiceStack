using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.Plugins.RateLimiting
{
    public abstract class RateLimitingWithMapPlugin : RateLimitingPlugin
    {
        protected abstract string RateLimitMapSettingKey { get; }

        protected readonly Dictionary<string, Dictionary<string, int>> RateLimitSettingMaps;

        protected RateLimitingWithMapPlugin()
            : base()
        {
            RateLimitSettingMaps = new Dictionary<string, Dictionary<string, int>>();
        }

        protected override int RateLimit
        {
            get
            {
                int rateLimit;
                string requestIdentity = GenerateRequestIdentity(HostContext.Instance.Request);
                if (!RateLimitSettingMaps[ServicePath].TryGetValue(requestIdentity, out rateLimit))
                    return base.RateLimit;
                else
                    return rateLimit;
            }
        }

        protected Dictionary<string, int> GetRateLimitSettingMap(string settingKey, Dictionary<string, int> initializeMap = null)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
                return new Dictionary<string, int>();

            Dictionary<string, int> rateLimitSettingMap = 
                initializeMap == null ? new Dictionary<string, int>() : new Dictionary<string, int>(initializeMap);

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

        protected override void Init()
        {
            base.Init();
            
            Dictionary<string, int> globalRateLimitMap = GetRateLimitSettingMap(RateLimitMapSettingKey);

            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string rateLimitSettingKey = metadata.GetServiceSpecificSettingKey(RateLimitSettingKey);
                string settingValue = ConfigurationManager.AppSettings[rateLimitSettingKey];
                int rateLimit;
                int.TryParse(settingValue, out rateLimit);
                Dictionary<string, int> initializeMap = rateLimit <= 0 ? globalRateLimitMap : null;
                string rateLimitMapSettingKey = metadata.GetServiceSpecificSettingKey(RateLimitMapSettingKey);
                RateLimitSettingMaps[metadata.ServicePath] = GetRateLimitSettingMap(rateLimitMapSettingKey, initializeMap);
            }
        }

        public override IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
            {
                { EnableRateLimitingCheckSettingKey, RateLimitingSettings[servicePath].Enabled },
                { RateLimitSettingKey, RateLimitingSettings[servicePath].RateLimit },
                { RateLimitMapSettingKey, RateLimitSettingMaps[servicePath]}
            };
        }

        internal Dictionary<string, int> GetRateLimitMap(string servicePath)
        {
            return RateLimitSettingMaps[servicePath];
        }

        public virtual void Refresh(string servicePath, bool? enabled = null, int? rateLimit = null, Dictionary<string, int> specificRateLimits = null)
        {
            if (enabled.HasValue)
                RateLimitingSettings[servicePath].Enabled = enabled.Value;

            if (rateLimit.HasValue && rateLimit.Value > 0)
                RateLimitingSettings[servicePath].RateLimit = rateLimit.Value;

            if (specificRateLimits != null && specificRateLimits.Count > 0)
                RateLimitSettingMaps[servicePath] = specificRateLimits;
        }
    }
}

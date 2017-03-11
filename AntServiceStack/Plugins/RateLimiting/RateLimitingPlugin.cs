using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.Common.Hystrix.Atomic;
using Freeway.Logging;
using AntServiceStack.Common.Configuration;

namespace AntServiceStack.Plugins.RateLimiting
{
    public abstract class RateLimitingPlugin : IPlugin, IHasConfigInfo
    {
        protected const int RateLimitingTimeSpan = 10;

        // Too Many Requests
        protected const int DefaultHttpStatusCode = 429;

        protected static string ServicePath
        {
            get
            {
                return HostContext.Instance.Request.ServicePath;
            }
        }

        protected virtual int DefaultRateLimit
        {
            get { return 100; }
        }

        protected virtual int StatusCode { get { return DefaultHttpStatusCode; } }

        protected readonly Dictionary<string, RateLimitingSetting> RateLimitingSettings;

        internal readonly Dictionary<string, RateLimitingBuffer> RateLimitingBuffers;

        protected abstract string EnableRateLimitingCheckSettingKey { get; }
        protected abstract string RateLimitSettingKey { get; }

        protected virtual bool Enabled
        {
            get
            {
                return RateLimitingSettings[ServicePath].Enabled;
            }
        }

        protected virtual int RateLimit
        {
            get
            {
                return RateLimitingSettings[ServicePath].RateLimit;
            }
        }

        protected virtual string Name 
        {
            get 
            {
                return "Rate Limiting";
            }
        }

        protected RateLimitingPlugin()
        {
            RateLimitingBuffers = new Dictionary<string, RateLimitingBuffer>();
            RateLimitingSettings = new Dictionary<string, RateLimitingSetting>();
        }

        protected int GetRateLimitSetting(string settingKey, int defaultValue)
        {
            string settingValue = ConfigurationManager.AppSettings[settingKey];
            int rate;
            int.TryParse(settingValue, out rate);
            if (rate <= 0)
                rate = defaultValue;
            return rate;
        }

        protected bool GetRateLimitEnableSetting(string settingKey, bool defaultValue = false)
        {
            string settingValue = ConfigurationManager.AppSettings[settingKey];
            bool enabled;
            if (!bool.TryParse(settingValue, out enabled))
                enabled = defaultValue;
            return enabled;
        }

        protected virtual void Init()
        {
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                RateLimitingBuffers[metadata.ServicePath] = new RateLimitingBuffer(RateLimitingTimeSpan);
            }

            bool defaultEnabled = GetRateLimitEnableSetting(EnableRateLimitingCheckSettingKey);
            int defaultRateLimit = GetRateLimitSetting(RateLimitSettingKey, DefaultRateLimit);

            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string enableRateLimitingCheckSettingKey = metadata.GetServiceSpecificSettingKey(EnableRateLimitingCheckSettingKey);
                string rateLimitSettingKey = metadata.GetServiceSpecificSettingKey(RateLimitSettingKey);
                
                RateLimitingSettings[metadata.ServicePath] = new RateLimitingSetting()
                {
                    Enabled = GetRateLimitEnableSetting(enableRateLimitingCheckSettingKey, defaultEnabled),
                    RateLimit = GetRateLimitSetting(rateLimitSettingKey, defaultRateLimit)
                };
            }
        }

        public void Register(IAppHost appHost)
        {
            Init();
            appHost.PreRequestFilters.Add(CheckRateLimiting);
            ConfigInfoHandler.RegisterConfigInfoOwner(this);
        }

        public virtual void CheckRateLimiting(IHttpRequest request, IHttpResponse response)
        {
            if (!Enabled)
                return;

            string requestIdentity = GenerateRequestIdentity(request);
            long requestTimeInSeconds = GetCurrentTimeInSeconds();
            RateLimitingBuffers[ServicePath].AddRate(requestIdentity, requestTimeInSeconds);
            int rate = RateLimitingBuffers[ServicePath].GetRate(requestIdentity, requestTimeInSeconds);
            if (rate <= RateLimit)
                return;

            RateLimitingBuffers[ServicePath].ReduceRate(requestIdentity, requestTimeInSeconds);

            string message = string.Format("{0} Check refused a request. Request Identity: {1}", Name, requestIdentity);
            ErrorUtils.LogError(message, request, default(Exception), false, "FXD300011");
            if (response.ExecutionResult != null)
                response.ExecutionResult.ValidationExceptionThrown = true;
            response.StatusCode = StatusCode;
            response.StatusDescription = message;
            response.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
            string traceIdString = request.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER];
            if (!string.IsNullOrWhiteSpace(traceIdString))
                response.AddHeader(ServiceUtils.TRACE_ID_HTTP_HEADER, traceIdString);
            response.LogRequest(request);
            response.EndRequest();
        }

        internal int GetCurrentCount(string servicePath, string identity)
        {
            long requestTimeInSeconds = GetCurrentTimeInSeconds();
            return RateLimitingBuffers[servicePath].GetRate(identity, requestTimeInSeconds); ;
        }

        protected long GetCurrentTimeInSeconds()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        protected abstract string GenerateRequestIdentity(IHttpRequest request);

        public virtual void Refresh(string servicePath, bool? enabled = null, int? rateLimit = null)
        {
            if (enabled.HasValue)
                RateLimitingSettings[servicePath].Enabled = enabled.Value;

            if (rateLimit.HasValue && rateLimit.Value > 0)
                RateLimitingSettings[servicePath].RateLimit = rateLimit.Value;
        }

        public virtual IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
            {
                { EnableRateLimitingCheckSettingKey, RateLimitingSettings[servicePath].Enabled },
                { RateLimitSettingKey, RateLimitingSettings[servicePath].RateLimit }
            };
        }

        internal bool IsEnable(string servicePath)
        {
            return RateLimitingSettings[servicePath].Enabled;
        }

        internal int GetRateLimit(string servicePath)
        {
            return RateLimitingSettings[servicePath].RateLimit;
        }

        protected class RateLimitingSetting
        {
            public bool Enabled { get; set; }
            public int RateLimit { get; set; }
        }
    }
}

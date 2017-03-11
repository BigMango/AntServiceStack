using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Configuration;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Types;

namespace AntServiceStack.Plugins.CrossDomain
{
    public class CrossDomainPlugin : IPlugin, IHasConfigInfo
    {
        protected const string EnableCrossDomainSupportSettingKey = "SOA.EnableCrossDomainSupport";

        internal bool IsEnable(string servicePath)
        {
            return _serviceEnabledMap[servicePath];
        }

        public void Register(IAppHost appHost)
        {
            Init();
            ConfigInfoHandler.RegisterConfigInfoOwner(this);
            appHost.PreRequestFilters.Add(FilterCrossDomain);
        }

        protected Dictionary<string, bool> _serviceEnabledMap = new Dictionary<string, bool>();

        protected virtual void Init()
        {
            string settingValue = ConfigurationManager.AppSettings[EnableCrossDomainSupportSettingKey];
            bool defaultEnabled;
            bool.TryParse(settingValue, out defaultEnabled);
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string enableCrossDomainSupportSettingKey = metadata.GetServiceSpecificSettingKey(EnableCrossDomainSupportSettingKey);
                settingValue = ConfigurationManager.AppSettings[enableCrossDomainSupportSettingKey];
                bool enabled;
                if (!bool.TryParse(settingValue, out enabled))
                    enabled = defaultEnabled;
                _serviceEnabledMap[metadata.ServicePath] = enabled;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>() { { EnableCrossDomainSupportSettingKey, _serviceEnabledMap[servicePath] }};
        }

        public void FilterCrossDomain(IHttpRequest request, IHttpResponse response)
        {
            if (!_serviceEnabledMap[request.ServicePath])
                return;

            if (request.Headers[HttpHeaders.Origin] != null)
            {
                response.AddHeader(HttpHeaders.AllowOrigin, request.Headers[HttpHeaders.Origin]);

                //preflight request
                if (request.HttpMethod == HttpMethods.Options
                    && (request.Headers[HttpHeaders.RequestMethod] != null || request.Headers[HttpHeaders.RequestHeaders] != null))
                {
                    response.AddHeader(HttpHeaders.AllowHeaders, request.Headers[HttpHeaders.RequestHeaders]);
                    response.AddHeader(HttpHeaders.AllowMethods, request.Headers[HttpHeaders.RequestMethod]);

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.EndRequest();
                }
            }
        }
    }
}

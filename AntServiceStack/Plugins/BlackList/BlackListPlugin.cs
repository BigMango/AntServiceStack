using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.Plugins.OperationInfo;
using AntServiceStack.Plugins.RouteInfo;

namespace AntServiceStack.Plugins.BlackList
{
    public abstract class BlackListPlugin : IPlugin, IHasConfigInfo
    {
        protected const string CheckHealthOperationName = "checkhealth";

        protected static readonly List<string> ExcludedPathControllers = new List<string>()
        {
            "metadata",
            HystrixInfoHandler.RestPath,
            RequestInfoHandler.RestPath,
            "__requestinfo",
            ConfigInfoHandler.RestPath,
            OperationInfoHandler.RestPath,
            RouteInfoHandler.RestPath
        };

        protected static readonly Dictionary<string, List<string>> ServiceSpecificExcludedPathControllers = new Dictionary<string, List<string>>();

        public static void ExcludePathController(string pathController)
        {
            ExcludePathController(pathController, null);
        }

        public static void ExcludePathController(string pathController, string servicePath)
        {
            if (string.IsNullOrWhiteSpace(pathController))
                return;

            pathController = pathController.Trim().ToLower();
            if (servicePath == null)
            {
                if (!ExcludedPathControllers.Contains(pathController))
                    ExcludedPathControllers.Add(pathController);
            }
            else
            {
                servicePath = servicePath.Trim().ToLower();
                if (!ServiceSpecificExcludedPathControllers.ContainsKey(servicePath))
                    ServiceSpecificExcludedPathControllers[servicePath] = new List<string>();
                if (!ServiceSpecificExcludedPathControllers[servicePath].Contains(pathController))
                    ServiceSpecificExcludedPathControllers[servicePath].Add(pathController);
            }
        }

        protected readonly Dictionary<string, BlackListSetting> BlackListSettings;

        protected abstract string EnableBlackListCheckSettingKey { get; }

        protected abstract string BlackListSettingKey { get; }

        protected virtual string Name
        {
            get
            {
                return "Black list";
            }
        }

        protected BlackListPlugin()
        {
            BlackListSettings = new Dictionary<string, BlackListSetting>();
        }

        protected void Init()
        {
            string settingValue = ConfigurationManager.AppSettings[EnableBlackListCheckSettingKey];
            bool defaultEnabled;
            bool.TryParse(settingValue, out defaultEnabled);

            List<string> defaultBlackList = new List<string>();
            settingValue = ConfigurationManager.AppSettings[BlackListSettingKey];
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                settingValue = settingValue.Trim();
                string[] blackList = settingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in blackList)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        defaultBlackList.Add(item.Trim());
                }
            }

            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string enableBlackListCheckSettingKey = metadata.GetServiceSpecificSettingKey(EnableBlackListCheckSettingKey);
                settingValue = ConfigurationManager.AppSettings[enableBlackListCheckSettingKey];
                bool enabled;
                if (!bool.TryParse(settingValue, out enabled))
                    enabled = defaultEnabled;

                string blackListSettingKey = metadata.GetServiceSpecificSettingKey(BlackListSettingKey);
                settingValue = ConfigurationManager.AppSettings[blackListSettingKey];
                List<string> serviceBlackList = new List<string>();
                if (!string.IsNullOrWhiteSpace(settingValue))
                {
                    settingValue = settingValue.Trim();
                    string[] blackList = settingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in blackList)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            serviceBlackList.Add(item.Trim());
                    }
                }
                else
                    serviceBlackList.AddRange(defaultBlackList);

                BlackListSettings[metadata.ServicePath] = new BlackListSetting()
                {
                    Enabled = enabled,
                    BlackList = serviceBlackList.Distinct().ToList()
                };
            }
        }

        public virtual void Register(IAppHost appHost)
        {
            Init();
            appHost.PreRequestFilters.Add(FilterBlackList);
            ConfigInfoHandler.RegisterConfigInfoOwner(this);
        }

        public virtual void FilterBlackList(IHttpRequest request, IHttpResponse response)
        {
            if (!BlackListSettings[request.ServicePath].Enabled)
                return;

            string pathInfo = request.PathInfo;
            if (!string.IsNullOrWhiteSpace(pathInfo))
            {
                pathInfo = pathInfo.Trim().ToLower();
                string[] pathParts = pathInfo.TrimStart('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length > 0)
                {
                    string pathController = pathParts[0].Trim();
                    if (ExcludedPathControllers.Contains(pathController))
                        return;
                    if (ServiceSpecificExcludedPathControllers.ContainsKey(request.ServicePath)
                        && ServiceSpecificExcludedPathControllers[request.ServicePath].Contains(pathController))
                        return;
                }
            }

            if (request.OperationName != null && request.OperationName.Trim().ToLower() == CheckHealthOperationName)
                return;

            string requestIdentity;
            if (ValidateRequest(request, out requestIdentity))
                return;

            string message = string.Format("{0} Check refused a request. Request Identity: {1}", Name, requestIdentity);
            ErrorUtils.LogError(message, request, default(Exception), false, "FXD300033");
            if (response.ExecutionResult != null)
                response.ExecutionResult.ValidationExceptionThrown = true;
            response.StatusCode = (int)HttpStatusCode.Forbidden;
            response.StatusDescription = message;
            response.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
            string traceIdString = request.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER];
            if (!string.IsNullOrWhiteSpace(traceIdString))
                response.AddHeader(ServiceUtils.TRACE_ID_HTTP_HEADER, traceIdString);
            response.LogRequest(request);
            response.EndRequest();
        }

        protected abstract bool ValidateRequest(IHttpRequest request, out string requestIdentity);

        public virtual void Refresh(string servicePath, bool? enabled = null, List<string> newBlackList = null)
        {
            if (enabled.HasValue)
                BlackListSettings[servicePath].Enabled = enabled.Value;

            if (newBlackList != null)
                BlackListSettings[servicePath].BlackList = newBlackList;
        }

        public IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
            {
                { EnableBlackListCheckSettingKey, BlackListSettings[servicePath].Enabled },
                { BlackListSettingKey, BlackListSettings[servicePath].BlackList }
            };
        }

        internal bool IsEnable(string servicePath)
        {
            return BlackListSettings[servicePath].Enabled;
        }

        internal List<string> GetBlackList(string servicePath)
        {
            return BlackListSettings[servicePath].BlackList;
        }

        protected class BlackListSetting
        {
            public bool Enabled { get; set; }
            public List<string> BlackList { get; set; }
        }
    }
}

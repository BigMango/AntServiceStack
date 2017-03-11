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

namespace AntServiceStack.Plugins.WhiteList
{
    public abstract class WhiteListPlugin : IPlugin, IHasConfigInfo
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

        protected readonly Dictionary<string, WhiteListSetting> WhiteListSettings;

        protected abstract string EnableWhiteListCheckSettingKey { get; }

        protected abstract string WhiteListSettingKey { get; }

        protected virtual string Name 
        {
            get
            {
                return "White list";
            }
        }

        protected WhiteListPlugin()
        {
            WhiteListSettings = new Dictionary<string, WhiteListSetting>();
        }

        protected void Init()
        {
            string settingValue = ConfigurationManager.AppSettings[EnableWhiteListCheckSettingKey];
            bool defaultEnabled;
            bool.TryParse(settingValue, out defaultEnabled);

            List<string> defaultWhiteList = new List<string>();
            settingValue = ConfigurationManager.AppSettings[WhiteListSettingKey];
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                settingValue = settingValue.Trim();
                string[] whiteList = settingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in whiteList)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        defaultWhiteList.Add(item.Trim());
                }
            }

            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string enableWhiteListCheckSettingKey = metadata.GetServiceSpecificSettingKey(EnableWhiteListCheckSettingKey);
                settingValue = ConfigurationManager.AppSettings[enableWhiteListCheckSettingKey];
                bool enabled;
                if (!bool.TryParse(settingValue, out enabled))
                    enabled = defaultEnabled;

                string whiteListSettingKey = metadata.GetServiceSpecificSettingKey(WhiteListSettingKey);
                settingValue = ConfigurationManager.AppSettings[whiteListSettingKey];
                List<string> serviceWhiteList = new List<string>();
                if (!string.IsNullOrWhiteSpace(settingValue))
                {
                    settingValue = settingValue.Trim();
                    string[] whiteList = settingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in whiteList)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            serviceWhiteList.Add(item.Trim());
                    }
                }
                else
                    serviceWhiteList.AddRange(defaultWhiteList);

                WhiteListSettings[metadata.ServicePath] = new WhiteListSetting()
                {
                    Enabled = enabled,
                    WhiteList = serviceWhiteList.Distinct().ToList()
                };
            }
        }

        public virtual void Register(IAppHost appHost)
        {
            Init();
            appHost.PreRequestFilters.Add(FilterWhiteList);
            ConfigInfoHandler.RegisterConfigInfoOwner(this);
        }

        public virtual void FilterWhiteList(IHttpRequest request, IHttpResponse response)
        {
            if (!WhiteListSettings[request.ServicePath].Enabled)
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
            ErrorUtils.LogError(message, request, default(Exception), false, "FXD300012");
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

        public virtual void Refresh(string servicePath, bool? enabled = null, List<string> newWhiteList = null)
        {
            if (enabled.HasValue)
                WhiteListSettings[servicePath].Enabled = enabled.Value;

            if (newWhiteList != null)
                WhiteListSettings[servicePath].WhiteList = newWhiteList;
        }

        public IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
            {
                { EnableWhiteListCheckSettingKey, WhiteListSettings[servicePath].Enabled },
                { WhiteListSettingKey, WhiteListSettings[servicePath].WhiteList }
            };
        }
        
        internal bool IsEnable(string servicePath)
        {
            return WhiteListSettings[servicePath].Enabled;
        }

        internal List<string> GetWhiteList(string servicePath)
        {
            return WhiteListSettings[servicePath].WhiteList;
        }

        protected class WhiteListSetting
        {
            public bool Enabled { get; set; }
            public List<string> WhiteList { get; set; }
        }
    }
}

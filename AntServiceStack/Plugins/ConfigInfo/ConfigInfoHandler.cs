using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Common.Hystrix.Strategy.Properties;
using AntServiceStack.Common.Utils;
using AntServiceStack.Text;


namespace AntServiceStack.Plugins.ConfigInfo
{
    public class ConfigInfoHandler : IHttpHandler, IServiceStackHttpHandler
    {
        public const string RestPath = "_configinfo";

        static readonly List<IHasConfigInfo> ConfigInfoOwners = new List<IHasConfigInfo>();

        public static void RegisterConfigInfoOwner(IHasConfigInfo configInfoOwner)
        {
            if (configInfoOwner == null)
                return;

            ConfigInfoOwners.Add(configInfoOwner);
        }

        string _servicePath;

        public ConfigInfoHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(_servicePath, typeof(ConfigInfoHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(ConfigInfoHandler).Name);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                return;

            List<KeyValuePair<string, object>> configInfo = new List<KeyValuePair<string, object>>();

            if (AppHostBase.Instance != null)
            {
                configInfo.Add(new KeyValuePair<string, object>("StartUpTime", AppHostBase.Instance.StartUpTime));
            }

            if (AppHostHttpListenerBase.Instance != null)
            {
                configInfo.Add(new KeyValuePair<string, object>("StartUpTime", AppHostHttpListenerBase.Instance.StartUpTime));
            }

            configInfo.Add(new KeyValuePair<string, object>("AppId", ServiceUtils.AppId));

            configInfo.Add(new KeyValuePair<string, object>("IPv4", ServiceUtils.HostIP));

            configInfo.Add(new KeyValuePair<string, object>("MachineName", ServiceUtils.MachineName));

            configInfo.Add(new KeyValuePair<string, object>("SOA.CurrentEnv", EnvironmentUtility.CurrentEnv));

            var metadata = EndpointHost.MetadataMap[httpReq.ServicePath];
            configInfo.Add(new KeyValuePair<string, object>("SOA.ServiceName", metadata.ServiceName));
            configInfo.Add(new KeyValuePair<string, object>("SOA.ServiceNamespace", metadata.ServiceNamespace));
            configInfo.Add(new KeyValuePair<string, object>("SOA.ServiceTestSubEnv", metadata.ServiceTestSubEnv ?? "null"));


            configInfo.Add(new KeyValuePair<string, object>(
                ServiceMetadata.DefaultLogErrorWithRequestInfoSettingKey,
                metadata.LogErrorWithRequestInfo));

            configInfo.Add(new KeyValuePair<string, object>(
                ServiceMetadata.DefaultLogCommonRequestInfoSettingKey,
                metadata.LogCommonRequestInfo));
            configInfo.Add(new KeyValuePair<string, object>(
                ServiceMetadata.DefaultLogH5HeadExtensionDataSettingKey,
                metadata.LogH5HeadExtensionData));

            bool circuitBreakerForceClosed = metadata.CircuitBreakerForceClosed;
            configInfo.Add(new KeyValuePair<string, object>(ServiceMetadata.DefaultCircuitBreakerForceClosedSettingKey, circuitBreakerForceClosed));

            Dictionary<string, double> timeoutMap = new Dictionary<string, double>();
            foreach (Operation operation in metadata.Operations)
                timeoutMap.Add(operation.Name, operation.HystrixCommand.GetExecutionTimeout().TotalMilliseconds);
            configInfo.Add(new KeyValuePair<string, object>(ServiceMetadata.DefaultOperationTimeoutMapSettingKey, timeoutMap));

            configInfo.Add(new KeyValuePair<string, object>(
                ServiceMetadata.DefaultUseChunkedTransferEncodingSettingKey, metadata.UseChunkedTransferEncoding));

            configInfo.Add(new KeyValuePair<string, object>("SOA.FxConfigWebServiceUtils.Enabled", FxConfigWebServiceUtils.Enabled));
            configInfo.Add(new KeyValuePair<string, object>("SOA.FxConfigWebServiceUtils.ApiUrl", FxConfigWebServiceUtils.ConfigWebServiceApiUrl));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MinGlobalDefaultTimeout", HystrixCommandHelper.MinGlobalDefaultCircuitBreakerTimeoutSetting.TotalMilliseconds.ToString()));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.GlobalDefaultTimeout",
                HystrixCommandHelper.GlobalDefaultCircuitBreakerTimeoutSetting.HasValue ? HystrixCommandHelper.GlobalDefaultCircuitBreakerTimeoutSetting.Value.TotalMilliseconds.ToString() : null));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.FrameworkDefaultTimeout", HystrixPropertiesCommandDefault.DefaultExecutionIsolationThreadTimeout.TotalMilliseconds.ToString()));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.FrameworkDefaultConnectionMaxRequestCount", ServiceMetadata.FrameworkDefaultConnectionMaxRequestCount));
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MinConnectionMaxRequestCount", ServiceMetadata.MinConnectionMaxRequestCount));
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.CheckConnectionMaxRequestCount", metadata.CheckConnectionMaxRequestCount));
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.ConnectionMaxRequestCount", metadata.ConnectionMaxRequestCount));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.RequestLogMaxSize", MessageLogConfig.RequestLogMaxSize));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.ResponseLogMaxSize", MessageLogConfig.ResponseLogMaxSize));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.FrameworkDefalut.Test", MessageLogConfig.FrameworkDefalutMessageLogConfigOfTestEnv));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.FrameworkDefalut.Uat", MessageLogConfig.FrameworkDefalutMessageLogConfigOfUatEnv));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.FrameworkDefalut.Prod", MessageLogConfig.FrameworkDefalutMessageLogConfigOfProdEnv));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.FrameworkDefalut.Null", MessageLogConfig.FrameworkDefalutMessageLogConfigOfNullEnv));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.FrameworkDefault.Current", MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig));

            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.Service", metadata.ServiceMessageLogConfig));

            var operationMessageConfigs = from operation in metadata.Operations
                                          select new 
                                          {
                                              operation.Name,
                                              operation.OperationMessageLogConfig,
                                          };
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.MessageLogConfig.Operation", operationMessageConfigs));
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.DeserializeRequestUseMemoryStream", metadata.DeserializeRequestUseMemoryStream));
            configInfo.Add(new KeyValuePair<string, object>(
                "SOA.LogPopulateExceptionAsWarning", ReflectionUtils.LogPopulateExceptionAsWarning));

            foreach (IHasConfigInfo configInfoOwner in ConfigInfoOwners)
                configInfo.AddRange(configInfoOwner.GetConfigInfo(httpReq.ServicePath));

            using (JsConfigScope scope = JsConfig.BeginScope())
            {
                scope.ExcludeTypeInfo = true;
                scope.DateHandler = JsonDateHandler.LongDateTime;
                httpRes.ContentType = "application/json";
                httpRes.Write(WrappedJsonSerializer.Instance.SerializeToString(configInfo));
            }
        }
    }
}

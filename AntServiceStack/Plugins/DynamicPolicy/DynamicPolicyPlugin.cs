using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Configuration;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Configuration;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.ServiceClient;
using AntServiceStack.Text;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.Plugins.WhiteList;
using AntServiceStack.Plugins.RateLimiting;
using AntServiceStack.Common.Hystrix.Strategy.Properties;
using Freeway.Logging;
using AntServiceStack.Plugins.BlackList;

namespace AntServiceStack.Plugins.DynamicPolicy
{
    /// <summary>
    /// 动态配置策略
    /// </summary>
    public class DynamicPolicyPlugin : IPlugin, IHasConfigInfo
    {
        //获取服务的地址
        const string PolicyServiceUrlSettingKey = "SOA.PolicyServiceUrl";
        //timer的Interva
        const string PolicyServiceSyncIntervalSettingKey = "SOA.DynamicPolicySyncInterval";
        const string EnablePolicyServiceSettingKey = "SOA.EnableDynamicPolicy";
        const int MinPolicyServiceSyncInterval = 60 * 1000;
        const int MinThreadPoolSize = 10;
        const int MaxThreadPoolSize = 1000;

        const string FxConfigWebServicePolicyServiceUrlSettingKey = "SOA_PolicyService_Url";

        static string _policyServiceUrl;
        static readonly int PolicyServiceSyncInterval = MinPolicyServiceSyncInterval;

        static readonly bool Enabled;

        static string _syncErrorTitle;
        static string SyncErrorTitle
        {
            get
            {
                if (_syncErrorTitle == null)
                    _syncErrorTitle = string.Format("Sync Dynamic Policy from policy service {0} failed.", _policyServiceUrl);

                return _syncErrorTitle;
            }
        }

        internal bool IsEnable(string servicePath)
        {
            return _serviceEnabledMap[servicePath] && !string.IsNullOrWhiteSpace(_policyServiceUrl);
        }

        internal string PolicyServiceUrl
        {
            get { return _policyServiceUrl; }
        }

        internal int SyncInterval
        {
            get { return PolicyServiceSyncInterval; }
        }

        static DynamicPolicyPlugin()
        {
            _policyServiceUrl = ConfigurationManager.AppSettings[PolicyServiceUrlSettingKey];
            if (string.IsNullOrWhiteSpace(_policyServiceUrl))
            {
                if (!FxConfigWebServiceUtils.Enabled)
                    return;

                SyncPolicyServiceUrl();
                if (string.IsNullOrWhiteSpace(_policyServiceUrl))
                    FxConfigWebServiceUtils.SubscribeFxWebServiceConfigUpdateEvent(SyncPolicyServiceUrl);
            }

            Enabled = true;

            string policyServiceSyncIntervalSetting = ConfigurationManager.AppSettings[PolicyServiceSyncIntervalSettingKey];
            if (!string.IsNullOrWhiteSpace(policyServiceSyncIntervalSetting))
            {
                int.TryParse(policyServiceSyncIntervalSetting, out PolicyServiceSyncInterval);
                if (PolicyServiceSyncInterval < MinPolicyServiceSyncInterval)
                    PolicyServiceSyncInterval = MinPolicyServiceSyncInterval;
            }
        }

        private static void SyncPolicyServiceUrl()
        {
            if (!string.IsNullOrWhiteSpace(_policyServiceUrl))
                return;

            string value = FxConfigWebServiceUtils.GetConfigItemValue(FxConfigWebServicePolicyServiceUrlSettingKey);
            if (!string.IsNullOrWhiteSpace(value))
                _policyServiceUrl = value.Trim();
        }

        ILog _log;
        PolicyServiceClient _policyServiceClient;
        string _dataTransferFormat;
        bool _isPolicySyncRunning;
        Dictionary<string, bool> _serviceEnabledMap;

        protected virtual void Init()
        {
            string settingValue = ConfigurationManager.AppSettings[EnablePolicyServiceSettingKey];
            bool defaultEnabled;
            if (!bool.TryParse(settingValue, out defaultEnabled))
                defaultEnabled = true;
            _serviceEnabledMap = new Dictionary<string, bool>();
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string enablePolicyServiceSettingKey = metadata.GetServiceSpecificSettingKey(EnablePolicyServiceSettingKey);
                settingValue = ConfigurationManager.AppSettings[enablePolicyServiceSettingKey];
                bool enabled;
                if (!bool.TryParse(settingValue, out enabled))
                    enabled = defaultEnabled;
                _serviceEnabledMap[metadata.ServicePath] = enabled && Enabled;
            }
        }

        public DynamicPolicyPlugin()
            : this("json")
        {
        }

        public DynamicPolicyPlugin(string dataTransferFormat)
        {
            _dataTransferFormat = dataTransferFormat;
        }

        public void Register(IAppHost appHost)
        {
            Init();
            ConfigInfoHandler.RegisterConfigInfoOwner(this);

            if (!Enabled)
                return;

            if (_serviceEnabledMap.Values.Count(enabled => enabled) == 0)
                return;

            _log = LogManager.GetLogger(typeof(DynamicPolicyPlugin));

            SyncDynamicSettings(null, null);
            var mTimer = new Timer();
            mTimer.Interval = PolicyServiceSyncInterval;//一分钟一次
            mTimer.Enabled = true;
            mTimer.AutoReset = true;
            mTimer.Elapsed += new ElapsedEventHandler(SyncDynamicSettings);
        }

        void SyncDynamicSettings(object sender, ElapsedEventArgs e)
        {
            if (_isPolicySyncRunning)
                return;

            _isPolicySyncRunning = true;
            try
            {
                if (string.IsNullOrWhiteSpace(_policyServiceUrl))
                    FxConfigWebServiceUtils.SyncConfig();

                if (string.IsNullOrWhiteSpace(_policyServiceUrl))
                    return;

                if (_policyServiceClient == null)
                {
                    _policyServiceClient = PolicyServiceClient.GetInstance(_policyServiceUrl);
                    _policyServiceClient.Format = _dataTransferFormat;//"json"
                    _policyServiceClient.Timeout = TimeSpan.FromSeconds(100);
                    _policyServiceClient.ReadWriteTimeout = TimeSpan.FromSeconds(300);
                    _policyServiceClient.EnableCHystrixSupport = false;
                }

                foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
                {
                    if (!_serviceEnabledMap[metadata.ServicePath])
                        continue;

                    //获取每个服务的配置 一般都是一个服务
                    GetServiceSettingsResponseType policySyncResponse = null;
                    try
                    {
                        policySyncResponse = _policyServiceClient.GetServiceSettings(new GetServiceSettingsRequestType()
                        {
                            ServiceName = metadata.ServiceName,
                            ServiceNamespace = metadata.ServiceNamespace
                        });
                    }
                    catch (Exception ex)
                    {
                        _log.Error(SyncErrorTitle, ex, new Dictionary<string, string>
                        {
                            { "ErrorCode", "FXD300014" },
                            { "ServiceName", metadata.ServiceName },
                            { "ServiceNamespace", metadata.ServiceNamespace }
                        });
                        continue;
                    }

                    try
                    {
                        metadata.LogErrorWithRequestInfo = policySyncResponse.ServiceSettings.LogErrorWithRequestInfo;
                        metadata.LogCommonRequestInfo = policySyncResponse.ServiceSettings.LogCommonRequestInfo;
                        metadata.CircuitBreakerForceClosed = policySyncResponse.ServiceSettings.CircuitBreakerForceClosed;

                        Dictionary<string, TimeSpan> timeoutSettings = GenerateOperationTimeoutDictionary(policySyncResponse.ServiceSettings.OperationTimeoutSettings);
                        foreach (Operation operation in metadata.Operations)
                        {
                            //把每个方法的电路设置为连接的状态
                            operation.HystrixCommand.Properties.CircuitBreakerForceClosed.Set(metadata.CircuitBreakerForceClosed);
                            string operationName = operation.Name.ToLower();
                            if (timeoutSettings.ContainsKey(operationName))
                                operation.HystrixCommand.Properties.ExecutionIsolationThreadTimeout.Set(timeoutSettings[operationName]);
                        }

                        foreach (IPlugin plugin in EndpointHost.Plugins)
                        {
                            if (plugin is WhiteListPlugin)
                            {
                                bool enabled;
                                List<string> newWhiteList;
                                if (plugin is AppIdWhiteListPlugin)
                                {
                                    enabled = policySyncResponse.ServiceSettings.EnableAppIdWhiteListCheck;
                                    newWhiteList = policySyncResponse.ServiceSettings.AppIdWhiteList;
                                }
                                else if (plugin is IPWhiteListPlugin)
                                {
                                    enabled = policySyncResponse.ServiceSettings.EnableIPWhiteListCheck;
                                    newWhiteList = policySyncResponse.ServiceSettings.IPWhiteList;
                                }
                                else
                                    continue;

                                WhiteListPlugin whiteListPlugin = plugin as WhiteListPlugin;
                                whiteListPlugin.Refresh(metadata.ServicePath, enabled, newWhiteList);
                            }
                            else if (plugin is BlackListPlugin)
                            {
                                bool enabled;
                                List<string> newBlackList;
                                if (plugin is AppIdBlackListPlugin)
                                {
                                    enabled = policySyncResponse.ServiceSettings.EnableAppIdBlackListCheck;
                                    newBlackList = policySyncResponse.ServiceSettings.AppIdBlackList;
                                }
                                else if (plugin is IPBlackListPlugin)
                                {
                                    enabled = policySyncResponse.ServiceSettings.EnableIPBlackListCheck;
                                    newBlackList = policySyncResponse.ServiceSettings.IPBlackList;
                                }
                                else
                                    continue;

                                BlackListPlugin blackListPlugin = plugin as BlackListPlugin;
                                blackListPlugin.Refresh(metadata.ServicePath, enabled, newBlackList);
                            }
                            else if (plugin is RateLimitingPlugin)
                            {
                                bool enabled;
                                int rateLimit;
                                if (plugin is RateLimitingWithMapPlugin)
                                {
                                    Dictionary<string, int> rateLimitMap;
                                    if (plugin is AppIdRateLimitingPlugin)
                                    {
                                        enabled = policySyncResponse.ServiceSettings.EnableAppIdRateLimitingCheck;
                                        rateLimit = policySyncResponse.ServiceSettings.AppIdRateLimit;
                                        rateLimitMap = policySyncResponse.ServiceSettings.AppIdRateLimitSettings.ToDictionary(item=>item.AppId, item=>item.RateLimit);
                                    }
                                    else if (plugin is IPRateLimitingPlugin)
                                    {
                                        enabled = policySyncResponse.ServiceSettings.EnableIPRateLimitingCheck;
                                        rateLimit = policySyncResponse.ServiceSettings.IPRateLimit;
                                        rateLimitMap = policySyncResponse.ServiceSettings.IPRateLimitSettings.ToDictionary(item => item.IP, item => item.RateLimit);
                                    }
                                    else
                                        continue;

                                    RateLimitingWithMapPlugin rateLimitingPlugin = plugin as RateLimitingWithMapPlugin;
                                    rateLimitingPlugin.Refresh(metadata.ServicePath, enabled, rateLimit, rateLimitMap);

                                    continue;
                                }
                                else
                                {
                                    if (plugin is ServiceRateLimitingPlugin)
                                    {
                                        enabled = policySyncResponse.ServiceSettings.EnableServiceRateLimitingCheck;
                                        rateLimit = policySyncResponse.ServiceSettings.ServiceRateLimit;
                                    }
                                    else if (plugin is OperationRateLimitingPlugin)
                                    {
                                        enabled = policySyncResponse.ServiceSettings.EnableOperationRateLimitingCheck;
                                        Dictionary<string, int> operationRateLimitMap = policySyncResponse.ServiceSettings.OperationRateLimitSettings
                                            .ToDictionary(i => i.OperationName.Trim().ToLower(), i => i.RateLimit);
                                        OperationRateLimitingPlugin operationRateLimitPlugin = plugin as OperationRateLimitingPlugin;
                                        operationRateLimitPlugin.Refresh(metadata.ServicePath, enabled, operationRateLimitMap);

                                        continue;
                                    }
                                    else
                                        continue;

                                    RateLimitingPlugin rateLimitingPlugin = plugin as RateLimitingPlugin;
                                    rateLimitingPlugin.Refresh(metadata.ServicePath, enabled, rateLimit);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _log.Fatal(
                            "Regression Bug",
                            ex,
                            new Dictionary<string, string>
                            {
                                { "ErrorCode", "FXD300015" },
                                { "ServiceName", metadata.ServiceName },
                                { "ServiceNamespace", metadata.ServiceNamespace },
                                { "Dynamic Policy Response", TypeSerializer.SerializeToString(policySyncResponse) }
                            });
                         }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }
            finally
            {
                _isPolicySyncRunning = false;
            }
        }

        Dictionary<string, TimeSpan> GenerateOperationTimeoutDictionary(List<OperationTimeoutSettingDTO> settings)
        {
            Dictionary<string, TimeSpan> dictionary = new Dictionary<string, TimeSpan>();
            foreach (OperationTimeoutSettingDTO setting in settings)
            {
                if (setting.Timeout <= 0)
                    continue;
                dictionary[setting.OperationName.ToLower()] = TimeSpan.FromMilliseconds(setting.Timeout);
            }

            return dictionary;
        }

        public IEnumerable<KeyValuePair<string, object>> GetConfigInfo(string servicePath)
        {
            return new Dictionary<string, object>()
            {
                { PolicyServiceUrlSettingKey, _policyServiceUrl },
                { PolicyServiceSyncIntervalSettingKey, PolicyServiceSyncInterval },
                { EnablePolicyServiceSettingKey, _serviceEnabledMap[servicePath] && !string.IsNullOrWhiteSpace(_policyServiceUrl) }
            };
        }
    }
}

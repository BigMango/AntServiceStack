namespace CHystrix.Config
{
    using CHystrix;
    using CHystrix.Utils;
    using CHystrix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime.CompilerServices;
    using System.Timers;

    internal static class HystrixConfigSyncManager
    {
        private static string _chystrixConfigWebServiceUrl;
        private static Timer _timer;
        private const string CHystrixConfigServiceName = "CHystrixConfigService";
        private const string CHystrixConfigServiceNamespace = "http://soa.ctrip.com/framework/chystrix/configservice/v1";
        private const string CHystrixRegistryServiceName = "CHystrixRegistryService";
        private const string CHystrixRegistryServiceNamespace = "http://soa.ctrip.com/framework/soa/chystrix/registryservice/v1";
        private const string ConfigWebServiceCHystrixConfigServiceUrlName = "CHystrix_ConfigService_Url";
        private const string ConfigWebServiceSettingKey = "FxConfigServiceUrl";
        private const string ConfigWebServiceSOARegistryServiceUrlName = "SOA_RegistryService_Url";
        private const string ConfigWebServiceUrlSuffix = "ServiceConfig/ConfigInfoes/Get/921807";
        private const string SOARegistryServiceOperationName = "LookupServiceUrl";
        public const int SyncConfigIntervalMilliseconds = 0x927c0;

        public static void Reset()
        {
            try
            {
                if (_timer != null)
                {
                    Timer timer = _timer;
                    _timer = null;
                    using (timer)
                    {
                        timer.Stop();
                    }
                }
            }
            catch
            {
            }
        }

        public static void Start()
        {
            ConfigWebServiceUrl = ConfigurationManager.AppSettings["FxConfigServiceUrl"];
            if (string.IsNullOrWhiteSpace(ConfigWebServiceUrl))
            {
                ConfigWebServiceUrl = null;
                CommonUtils.Log.Log(LogLevelEnum.Fatal, "No FxConfigWebService setting is found in appSettings.", new Dictionary<string, string>().AddLogTagData("FXD303029"));
            }
            else
            {
                ConfigWebServiceUrl = ConfigWebServiceUrl.Trim();
                _chystrixConfigWebServiceUrl = ConfigWebServiceUrl.WithTrailingSlash() + "ServiceConfig/ConfigInfoes/Get/921807";
                if (_timer == null)
                {
                    SyncFXConfigWebServiceSettings();
                    string str = SyncSOAServiceUrl("CHystrixRegistryService", "http://soa.ctrip.com/framework/soa/chystrix/registryservice/v1");
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        HystrixCommandBase.RegistryServiceUrl = str;
                    }
                    Timer timer = new Timer {
                        Interval = 600000.0,
                        AutoReset = true,
                        Enabled = true
                    };
                    _timer = timer;
                    _timer.Elapsed += new ElapsedEventHandler(HystrixConfigSyncManager.SyncConfig);
                }
            }
        }

        private static void SyncConfig(object sender, ElapsedEventArgs arg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SOARegistryServiceUrl))
                {
                    SyncFXConfigWebServiceSettings();
                }
                string str = SyncSOAServiceUrl("CHystrixConfigService", "http://soa.ctrip.com/framework/chystrix/configservice/v1");
                if (!string.IsNullOrWhiteSpace(str))
                {
                    HystrixCommandBase.ConfigServiceUrl = str;
                }
                str = SyncSOAServiceUrl("CHystrixRegistryService", "http://soa.ctrip.com/framework/soa/chystrix/registryservice/v1");
                if (!string.IsNullOrWhiteSpace(str))
                {
                    HystrixCommandBase.RegistryServiceUrl = str;
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Hystrix Config Sync failed", exception, new Dictionary<string, string>().AddLogTagData("FXD303045"));
            }
        }

        private static void SyncFXConfigWebServiceSettings()
        {
            try
            {
                string str = _chystrixConfigWebServiceUrl.GetJsonFromUrl(null, null);
                if (string.IsNullOrWhiteSpace(str))
                {
                    CommonUtils.Log.Log(LogLevelEnum.Warning, "Got null response from config web service: " + _chystrixConfigWebServiceUrl, new Dictionary<string, string>().AddLogTagData("FXD303030"));
                }
                else
                {
                    List<ConfigWebServiceConfigItem> list = str.FromJson<List<ConfigWebServiceConfigItem>>();
                    if ((list == null) || (list.Count == 0))
                    {
                        CommonUtils.Log.Log(LogLevelEnum.Warning, "Response has no config data: " + _chystrixConfigWebServiceUrl, new Dictionary<string, string>().AddLogTagData("FXD303031"));
                    }
                    else
                    {
                        foreach (ConfigWebServiceConfigItem item in list)
                        {
                            if (((item != null) && (string.Compare(item.Name, "CHystrix_ConfigService_Url", true) == 0)) && !string.IsNullOrWhiteSpace(item.Value))
                            {
                                HystrixCommandBase.ConfigServiceUrl = item.Value.Trim();
                            }
                            if (((item != null) && (string.Compare(item.Name, "SOA_RegistryService_Url", true) == 0)) && !string.IsNullOrWhiteSpace(item.Value))
                            {
                                SOARegistryServiceUrl = item.Value.Trim();
                            }
                        }
                        if (string.IsNullOrWhiteSpace(SOARegistryServiceUrl) || string.IsNullOrWhiteSpace(HystrixCommandBase.ConfigServiceUrl))
                        {
                            CommonUtils.Log.Log(LogLevelEnum.Warning, "No config url is got from config web service: " + _chystrixConfigWebServiceUrl, new Dictionary<string, string>().AddLogTagData("FXD303032"));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to sync config from config web service: " + _chystrixConfigWebServiceUrl, exception, new Dictionary<string, string>().AddLogTagData("FXD303033"));
            }
        }

        private static string SyncSOAServiceUrl(string serviceName, string serviceNamespace)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SOARegistryServiceUrl))
                {
                    return null;
                }
                string url = SOARegistryServiceUrl.WithTrailingSlash() + "LookupServiceUrl.json";
                LookupServiceUrlRequest request = new LookupServiceUrlRequest {
                    ServiceName = serviceName,
                    ServiceNamespace = serviceNamespace
                };
                string str2 = url.PostJsonToUrl(request.ToJson(), null, null);
                if (string.IsNullOrWhiteSpace(str2))
                {
                    return null;
                }
                LookupServiceUrlResponse response = str2.FromJson<LookupServiceUrlResponse>();
                if ((response == null) || string.IsNullOrWhiteSpace(response.targetUrl))
                {
                    return null;
                }
                return response.targetUrl;
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to sync SOA service url from SOA registry service: " + SOARegistryServiceUrl, exception, new Dictionary<string, string>().AddLogTagData("FXD303046"));
                return null;
            }
        }

        public static string ConfigWebServiceUrl
        {
            get; private set; }

        public static string SOARegistryServiceUrl
        {
            get; private set;
        }
    }
}


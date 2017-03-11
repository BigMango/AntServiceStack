using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

using Freeway.Logging;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Utils;
using AntServiceStack.Text;

namespace AntServiceStack.Common.Configuration
{
    public static class FxConfigWebServiceUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FxConfigWebServiceUtils));

        const string ConfigWebServiceUrlSettingKey = "FxConfigServiceUrl";
        const string ConfigWebServiceUrlSuffix = "ServiceConfig/ConfigInfoes/Get/921803";

        public static string ConfigWebServiceApiUrl { get; private set; }

        public static bool Enabled { get; private set; }
        private static readonly ConcurrentDictionary<string, string> FxConfigItems = new ConcurrentDictionary<string, string>(
            StringComparer.InvariantCultureIgnoreCase);

        private static object _syncLock = new object();

        private static event Action OnFxWebServiceConfigUpdated;

        public static void SubscribeFxWebServiceConfigUpdateEvent(Action onUpdate)
        {
            if (!Enabled)
                return;

            OnFxWebServiceConfigUpdated += onUpdate;
        }

        static FxConfigWebServiceUtils()
        {
            try
            {
                //从配置文件里面读取FxConfigServiceUrl
                string configWebServiceUrl = ConfigurationManager.AppSettings[ConfigWebServiceUrlSettingKey];
                if (string.IsNullOrWhiteSpace(configWebServiceUrl))
                    return;

                Enabled = true;

                ConfigWebServiceApiUrl = configWebServiceUrl.Trim().WithTrailingSlash() + ConfigWebServiceUrlSuffix;
                SyncConfig();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to enable config web service sync: " + ConfigWebServiceApiUrl, ex,
                    new Dictionary<string, string>().AddErrorCode("FXD300026"));
            }
        }

        public static string GetConfigItemValue(string configItemName)
        {
            try
            {
                if (!Enabled)
                    return null;

                if (string.IsNullOrWhiteSpace(configItemName))
                    return null;

                string value;
                FxConfigItems.TryGetValue(configItemName, out value);
                return value;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to get config web service setting: " + configItemName, ex,
                    new Dictionary<string, string>().AddErrorCode("FXD300027"));
                return null;
            }
        }

        /// <summary>
        /// 同步配置
        /// </summary>
        public static void SyncConfig()
        {
            try
            {
                if (!Enabled)
                    return;

                if (!Monitor.TryEnter(_syncLock))
                    return;

                try
                {
                    string responseJson = ConfigWebServiceApiUrl.GetJsonFromUrl();
                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        Log.Warn("Got null response from config web service: " + ConfigWebServiceApiUrl,
                            new Dictionary<string, string>().AddErrorCode("FXD300019"));
                        return;
                    }
                    List<ConfigWebServiceConfigItem> response = responseJson.FromJson<List<ConfigWebServiceConfigItem>>();
                    if (response == null || response.Count == 0)
                    {
                        Log.Warn("Response has no config data: " + ConfigWebServiceApiUrl,
                            new Dictionary<string, string>().AddErrorCode("FXD300020"));
                        return;
                    }

                    foreach (ConfigWebServiceConfigItem item in response)
                    {
                        if (item != null && !string.IsNullOrWhiteSpace(item.Name))
                            FxConfigItems[item.Name] = item.Value;
                    }

                    if (OnFxWebServiceConfigUpdated != null)
                        OnFxWebServiceConfigUpdated();
                }
                finally
                {
                    Monitor.Exit(_syncLock);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to sync config from config web service: " + ConfigWebServiceApiUrl, ex,
                    new Dictionary<string, string>().AddErrorCode("FXD300021"));
            }
        }
    }

    [DataContract]
    internal class ConfigWebServiceConfigItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}

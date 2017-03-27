using System;
using System.Configuration;

namespace AntServiceStack.Manager.Common
{
    public static class ConfigUtil
    {
        private static readonly AppSettingsReader configurationAppSettings;



        static ConfigUtil()
        {
            configurationAppSettings = new System.Configuration.AppSettingsReader();
        }

        public static T GetConfig<T>(string key, T defaultValue)
        {
            try
            {
                var result = configurationAppSettings.GetValue(key, typeof(T));
                return (T)result;
            }
            catch (Exception)
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }
                return default(T);
            }
        }

        public static T GetConfig<T>(string key)
        {
            try
            {
                var result = configurationAppSettings.GetValue(key, typeof(T));
                return (T)result;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("没有在配置文件中的appSettings中找到{0}的配置，请检查配置文件配置！", key));
            }
        }

        public static string GetConfig(string key)
        {
            try
            {
                var result = configurationAppSettings.GetValue(key, typeof(string));
                return (string)result;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("没有在配置文件中的appSettings中找到{0}的配置，请检查配置文件配置！", key));
            }
        }
    }

}

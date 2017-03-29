using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Config;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.Client.Config
{
    internal class ClientConfig
    {
        public static ClientConfig Instance { get; private set; }

        static ClientConfig()
        {
            Instance = new ClientConfig();
        }

        public ClientConfig()
        {
            IConfigurationSource defaultValueConfigurationSource = BuildDefaultValueConfigurationSource();

            IConfiguration appSettingConfiguration = ObjectFactory.CreateAppSettingConfiguration();
            IConfigurationSource appSettingConfigurationSource = new DefaultConfigurationSource(0, "appSetting", appSettingConfiguration);

            ConfigurationManager = ObjectFactory.CreateDefaultConfigurationManager(
                appSettingConfigurationSource,
                defaultValueConfigurationSource);
        }

        public IConfigurationManager ConfigurationManager { get; private set; }

        private IConfigurationSource BuildDefaultValueConfigurationSource()
        {
            IMemoryConfiguration defaultValueConfiguration;
            switch (EnvironmentUtility.CurrentEnv)
            {
                case EnvironmentUtility.ProdEnv:
                    defaultValueConfiguration = BuildProdEnvDefaultValueConfiguration();
                    break;
                case EnvironmentUtility.UatEnv:
                    defaultValueConfiguration = BuildUatEnvDefaultValueConfiguration();
                    break;
                default:
                    defaultValueConfiguration = BuildTestEnvDefaultValueConfiguration();
                    break;
            }
            return ObjectFactory.CreateDefaultConfigurationSource(int.MinValue, "SOA.Client.DefaultValueConfigurationSource", defaultValueConfiguration);
        }

        private IMemoryConfiguration BuildTestEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
           // defaultValueConfiguration["] = "http://ant.soa.com/XXXX-service/";
            return defaultValueConfiguration;
        }

        private IMemoryConfiguration BuildUatEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
           // defaultValueConfiguration["] = "http://ant.soa.com/XXXX-service/";
            return defaultValueConfiguration;
        }

        private IMemoryConfiguration BuildProdEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
           // defaultValueConfiguration["] = "http://ant.soa.com/XXXX-service/";
            return defaultValueConfiguration;
        }
    }
}

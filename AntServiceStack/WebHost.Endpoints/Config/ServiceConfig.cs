using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Config;
using AntServiceStack.WebHost.Endpoints.Registry;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Configuration;

namespace AntServiceStack.WebHost.Endpoints.Config
{
    internal class ServiceConfig
    {
        public static ServiceConfig Instance { get; private set; }

        static ServiceConfig()
        {
            Instance = new ServiceConfig();
        }

        public ServiceConfig()
        {
            IConfigurationSource defaultValueConfigurationSource = BuildDefaultValueConfigurationSource();

            IConfiguration frameworkFoundationConfiguration = new FrameworkFoundationConfiguration();
            IConfigurationSource frameworkFoundationConfigurationSource = ObjectFactory.CreateDefaultConfigurationSource(-1, "frameworkFoundationConfiguration", frameworkFoundationConfiguration);

            IConfiguration appSettingConfiguration = ObjectFactory.CreateAppSettingConfiguration();
            IConfigurationSource appSettingConfigurationSource = ObjectFactory.CreateDefaultConfigurationSource(0, "appSetting", appSettingConfiguration);


            ConfigurationManager = ObjectFactory.CreateDefaultConfigurationManager(
                appSettingConfigurationSource,
                frameworkFoundationConfigurationSource,
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
            return ObjectFactory.CreateDefaultConfigurationSource(int.MinValue, "SOA.Service.DefaultValueConfigurationSource", defaultValueConfiguration);
        }

        private IMemoryConfiguration BuildTestEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "dev";
            return defaultValueConfiguration;
        }

        private IMemoryConfiguration BuildUatEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "uat";
            return defaultValueConfiguration;
        }

        private IMemoryConfiguration BuildProdEnvDefaultValueConfiguration()
        {
            var defaultValueConfiguration = new MemoryConfiguration();
            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "prod";
            return defaultValueConfiguration;
        }
    }
}

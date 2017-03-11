//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AntServiceStack.WebHost.Endpoints.Registry;
//using AntServiceStack.ServiceHost;
//using AntServiceStack.Common.Configuration;

//namespace AntServiceStack.WebHost.Endpoints.Config
//{
//    internal class ServiceConfig
//    {
//        public static ServiceConfig Instance { get; private set; }

//        static ServiceConfig()
//        {
//            Instance = new ServiceConfig();
//        }

//        public ServiceConfig()
//        {
//            IConfigurationSource defaultValueConfigurationSource = BuildDefaultValueConfigurationSource();

//            IConfiguration frameworkFoundationConfiguration = new FrameworkFoundationConfiguration();
//            IConfigurationSource frameworkFoundationConfigurationSource = ObjectFactory.CreateDefaultConfigurationSource(-1, "frameworkFoundationConfiguration", frameworkFoundationConfiguration);

//            IConfiguration appSettingConfiguration = ObjectFactory.CreateAppSettingConfiguration();
//            IConfigurationSource appSettingConfigurationSource = ObjectFactory.CreateDefaultConfigurationSource(0, "appSetting", appSettingConfiguration);

//            IDynamicConfiguration artemisApolloConfiguration = new ApolloConfiguration("FX.soa.artemis.client");
//            IConfigurationSource artemisApolloConfigurationSource = ObjectFactory.CreateDefaultDynamicConfigurationSource(10, "FX.soa.artemis.client", artemisApolloConfiguration);

//            IDynamicConfiguration soaApolloConfiguration = new ApolloConfiguration("FX.soa");
//            IConfigurationSource soaApolloConfigurationSource = ObjectFactory.CreateDefaultDynamicConfigurationSource(12, "FX.soa", soaApolloConfiguration);

//            ConfigurationManager = ObjectFactory.CreateDefaultConfigurationManager(
//                soaApolloConfigurationSource,
//                artemisApolloConfigurationSource,
//                appSettingConfigurationSource,
//                frameworkFoundationConfigurationSource,
//                defaultValueConfigurationSource);
//        }

//        public IConfigurationManager ConfigurationManager { get; private set; }

//        private IConfigurationSource BuildDefaultValueConfigurationSource()
//        {
//            IMemoryConfiguration defaultValueConfiguration;
//            switch (EnvironmentUtility.CurrentEnv)
//            {
//                case EnvironmentUtility.ProdEnv: 
//                    defaultValueConfiguration = BuildProdEnvDefaultValueConfiguration(); 
//                    break;
//                case EnvironmentUtility.UatEnv: 
//                    defaultValueConfiguration = BuildUatEnvDefaultValueConfiguration(); 
//                    break;
//                default: 
//                    defaultValueConfiguration = BuildTestEnvDefaultValueConfiguration(); 
//                    break;
//            }
//            return ObjectFactory.CreateDefaultConfigurationSource(int.MinValue, "SOA.Service.DefaultValueConfigurationSource", defaultValueConfiguration);
//        }

//        private IMemoryConfiguration BuildTestEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "dev";
//            defaultValueConfiguration[ArtemisServiceConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.fws.qa.nt.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }

//        private IMemoryConfiguration BuildUatEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "uat";
//            defaultValueConfiguration[ArtemisServiceConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.uat.qa.nt.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }

//        private IMemoryConfiguration BuildProdEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[ServiceMetadata.SERVICE_REGISTRY_ENV_KEY] = "prod";
//            defaultValueConfiguration[ArtemisServiceConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }
//    }
//}

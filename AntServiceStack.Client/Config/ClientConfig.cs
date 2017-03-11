//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AntServiceStack.Client.LoadBalancer;
//using Com.Ctrip.Soa.Caravan.Configuration;
//using Com.Ctrip.Soa.Caravan.Utility;
//using Com.Ctrip.Soa.Caravan.Configuration.Source;
//using AntServiceStack.Common.Utils;
//using AntServiceStack.ServiceClient;

//namespace AntServiceStack.Client.Config
//{
//    internal class ClientConfig
//    {
//        public static ClientConfig Instance { get; private set; }

//        static ClientConfig()
//        {
//            Instance = new ClientConfig();
//        }

//        public ClientConfig()
//        {
//            IConfigurationSource defaultValueConfigurationSource = BuildDefaultValueConfigurationSource();

//            IConfiguration appSettingConfiguration = ObjectFactory.CreateAppSettingConfiguration();
//            IConfigurationSource appSettingConfigurationSource = new DefaultConfigurationSource(0, "appSetting", appSettingConfiguration);

//            IDynamicConfiguration artemisApolloConfiguration = new ApolloConfiguration("FX.soa.artemis.client");
//            IConfigurationSource artemisApolloConfigurationSource = new DefaultDynamicConfigurationSource(10, "FX.soa.artemis.client", artemisApolloConfiguration);

//            IDynamicConfiguration ribbonApolloConfiguration = new ApolloConfiguration("FX.soa.caravan.ribbon");
//            IConfigurationSource ribbonApolloConfigurationSource = new DefaultDynamicConfigurationSource(11, "FX.soa.caravan.ribbon", ribbonApolloConfiguration);

//            IDynamicConfiguration soaApolloConfiguration = new ApolloConfiguration("FX.soa");
//            IConfigurationSource soaApolloConfigurationSource = new DefaultDynamicConfigurationSource(12, "FX.soa", soaApolloConfiguration);

//            ConfigurationManager = ObjectFactory.CreateDefaultConfigurationManager(
//                soaApolloConfigurationSource,
//                ribbonApolloConfigurationSource,
//                artemisApolloConfigurationSource,
//                appSettingConfigurationSource,
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
//            defaultValueConfiguration[LoadBalancerConstants.DataFolderPropertyKey] = @"D:\WebSites\CtripAppTemp\soa\ribbon\" + ServiceUtils.AppId;
//            return ObjectFactory.CreateDefaultConfigurationSource(int.MinValue, "SOA.Client.DefaultValueConfigurationSource", defaultValueConfiguration);
//        }

//        private IMemoryConfiguration BuildTestEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[LoadBalancerConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.fws.qa.nt.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }

//        private IMemoryConfiguration BuildUatEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[LoadBalancerConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.uat.qa.nt.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }

//        private IMemoryConfiguration BuildProdEnvDefaultValueConfiguration()
//        {
//            var defaultValueConfiguration = new MemoryConfiguration();
//            defaultValueConfiguration[LoadBalancerConstants.ArtemisUrlPropertyKey] = "http://artemis.soa.fx.ctripcorp.com/artemis-service/";
//            return defaultValueConfiguration;
//        }
//    }
//}

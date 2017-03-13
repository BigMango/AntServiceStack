//using System.Collections.Generic;
//using System.Collections.Concurrent;
//using System.Linq;
//using Com.ant.Soa.Artemis.Client;
//using Com.ant.Soa.Artemis.Common;
//using AntServiceStack.ServiceHost;
//using Freeway.Logging;
//using Com.ant.Soa.Caravan.Utility;
//using Com.ant.Soa.Caravan.Configuration;
//using AntServiceStack.WebHost.Endpoints.Config;

//namespace AntServiceStack.WebHost.Endpoints.Registry
//{
//    internal class HealthCheckRegistryFilter : IRegistryFilter
//    {
//        private static ILog _logger = LogManager.GetLogger(typeof(ArtemisServiceRegistry));

//        private static readonly object LockObject = new object();
//        private static HealthCheckRegistryFilter _instance;
//        public static HealthCheckRegistryFilter Instance
//        {
//            get
//            {
//                if (_instance == null)
//                {
//                    lock (LockObject)
//                    {
//                        if (_instance == null)
//                        {
//                            _instance = new HealthCheckRegistryFilter();
//                        }
//                    }
//                }
//                return _instance;
//            }
//        }

//        private ConcurrentDictionary<string, HealthCheckContext> _healthCheckContextMap;

//        private IProperty<bool> _globalSelfCheckProperty;
//        private ConcurrentDictionary<string, IProperty<bool>> _selfCheckPropertyMap;

//        private HealthCheckRegistryFilter()
//        {
//            _healthCheckContextMap = new ConcurrentDictionary<string, HealthCheckContext>();

//            _globalSelfCheckProperty = ServiceConfig.Instance.ConfigurationManager.GetProperty("soa.service.self-check.enabled", true);
//            _selfCheckPropertyMap = new ConcurrentDictionary<string, IProperty<bool>>();
//        }

//        public string RegistryFilterId
//        {
//            get { return "HealthCheckRegistryFilter"; }
//        }

//        public void Filter(List<Instance> instances)
//        {
//            instances.RemoveAll(instance => !IsHealthy(instance));
//        }

//        public bool IsHealthy(Instance instance)
//        {
//            if (!IsSelfCheckEnabled(instance.ServiceId))
//            {
//                _logger.Info(string.Format("Self check of service {0} is disabled.", instance.ServiceId));
//                return true;
//            }

//            if (instance == null)
//                return false;

//            HealthCheckContext healthCheckContext;
//            if (!_healthCheckContextMap.TryGetValue(instance.ServiceId, out healthCheckContext))
//            {
//                var message = string.Format("Unable to fine service metadata for instance: {0}", instance);
//                var tags = new Dictionary<string, string>();
//                tags["ServiceId"] = instance.ServiceId;
//                tags["InstanceId"] = instance.InstanceId;
//                _logger.Error(message, tags);
//                return true;
//            }

//            return healthCheckContext.IsHealthy;
//        }

//        public void RegisterService(ServiceMetadata serviceMetadata)
//        {
//            string serviceId = serviceMetadata.RefinedFullServiceName;
//            _healthCheckContextMap[serviceId] = new HealthCheckContext(serviceMetadata);

//            var configManager = ServiceConfig.Instance.ConfigurationManager;
//            var propertyKey = string.Format("soa.service.{0}.self-check.enabled", serviceId);
//            _selfCheckPropertyMap[serviceId] = configManager.GetProperty<bool>(propertyKey, true);
//        }

//        private bool IsSelfCheckEnabled(string serviceId)
//        {
//            if (!_globalSelfCheckProperty.Value)
//                return false;

//            IProperty<bool> property;
//            return !_selfCheckPropertyMap.TryGetValue(serviceId, out property) || property.Value;
//        }
//    }
//}

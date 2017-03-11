//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AntServiceStack.Common.Utils;
//using AntServiceStack.Text;
//using System.Web;
//using AntServiceStack.ServiceHost;
//using System.Reflection;
//using AntServiceStack.Common.Extensions;
//using Freeway.Logging;
//using System.Web.Hosting;
//using Microsoft.Web.Administration;
//using System.Diagnostics;
//using System.Management;
//using AntServiceStack.WebHost.Endpoints.Registry.Tools;
//using AntServiceStack.WebHost.Endpoints.Config;

//namespace AntServiceStack.WebHost.Endpoints.Registry
//{
//    class ArtemisServiceRegistry
//    {
//        private static ILog _logger = LogManager.GetLogger(typeof(ArtemisServiceRegistry));
//        private const string HTTP_PROTOCOL = "http";
//        private const string HTTPS_PROTOCOL = "https";

//        private const string PortKey = "Port";
//        private const string PhysicalPathKey = "PhysicalPath";
//        private const string VirtualPathKey = "VirtualPath";

//        private static CommandSwitch[] switches = new CommandSwitch[]
//        {
//            new CommandSwitch(PortKey, "port"),
//            new CommandSwitch(PhysicalPathKey, "path"),
//            new CommandSwitch(VirtualPathKey, "vpath"),
//        };

//        private static string _virtualPath = HostingEnvironment.ApplicationVirtualPath;
//        private static string _ipAddress = ServiceUtils.HostIP;
//        private static string _protocol;
//        private static int _port;

//        internal static bool AddPortToInstanceId { get; set; }
        
//        public static ArtemisServiceRegistry Instance { get; private set; }



//        static ArtemisServiceRegistry()
//        {
//            Instance = new ArtemisServiceRegistry();
//        }

//        private ArtemisServiceRegistry()
//        {
//            var artemisServiceUrlProperty = ServiceConfig.Instance.ConfigurationManager.GetProperty(ArtemisServiceConstants.ArtemisUrlPropertyKey);
//            if (string.IsNullOrWhiteSpace(artemisServiceUrlProperty.Value))
//            {
//                var errorMsg = "Missing artemis service url setting!";
//                _logger.Fatal(errorMsg, new Dictionary<string, string>() { { "ErrorCode", "FXD300076" } });
//                throw new ArgumentNullException(errorMsg);
//            }
//            _logger.Info(artemisServiceUrlProperty.Key + " is " + artemisServiceUrlProperty.Value, new Dictionary<string, string>() { { "ErrorCode", "FXD300076" } });

//            List<IRegistryFilter> registryFilters = new List<IRegistryFilter>();
//            registryFilters.Add(HealthCheckRegistryFilter.Instance);
//            RegistryClientConfig registryClientConfig = new RegistryClientConfig(registryFilters);
//            _artemisClientManagerConfig = new ArtemisClientManagerConfig(ServiceConfig.Instance.ConfigurationManager, 
//                ArtemisServiceConstants.EventMetricManager,
//                ArtemisServiceConstants.AuditMetricManager, 
//                registryClientConfig);

//            _manager = ArtemisClientManager.getManager(ArtemisServiceConstants.ManagerId, _artemisClientManagerConfig);
//        }

//        private string GetServiceUrl(ServiceMetadata serviceMetadata)
//        {
//            StringBuilder builder = new StringBuilder();
//            if (_port == 80)
//                builder.AppendFormat("{0}://{1}{2}", _protocol, _ipAddress, _virtualPath);
//            else
//                builder.AppendFormat("{0}://{1}:{2}{3}", _protocol, _ipAddress, _port, _virtualPath);
//            if (!_virtualPath.EndsWith("/"))
//                builder.Append("/");

//            if (HostingEnvironment.IsHosted && !string.IsNullOrWhiteSpace(EndpointHost.Config.ServiceStackHandlerFactoryPath))
//            {
//                builder.Append(EndpointHost.Config.ServiceStackHandlerFactoryPath);
//                builder.Append("/");
//            }
//            if (!string.IsNullOrWhiteSpace(serviceMetadata.ServicePath))
//            {
//                builder.Append(serviceMetadata.ServicePath);
//                builder.Append("/");
//            }
//            return builder.ToString();
//        }

//        private Instance CreateInstance(ServiceMetadata serviceMetadata)
//        {
//            var serviceUrl = GetServiceUrl(serviceMetadata);

//            Dictionary<string, string> metadata = null;
//            if (!string.IsNullOrWhiteSpace(serviceMetadata.ServiceTestSubEnv))
//            {
//                metadata = new Dictionary<string, string>();
//                metadata[ArtemisConstants.ArtemisSubEnvKey] = serviceMetadata.ServiceTestSubEnv;
//            }
//            string instanceId = ServiceUtils.HostIP;
//            if (AddPortToInstanceId)
//                instanceId += ":" + _port;
//            return new Instance()
//            {
//                RegionId = DeploymentConfig.RegionId,
//                ZoneId = DeploymentConfig.ZoneId,
//                ServiceId = serviceMetadata.RefinedFullServiceName,
//                Url = serviceUrl,
//                InstanceId = instanceId,
//                Protocol = _protocol,
//                IP = ServiceUtils.HostIP,
//                Port = _port,
//                MachineName = ServiceUtils.MachineName,
//                HealthCheckUrl = serviceUrl.WithTrailingSlash() + "checkhealth.json",
//                Status = Com.Ctrip.Soa.Artemis.Common.Instance.STATUS.UP,
//                Metadata = metadata,
//            };
//        }

//        public void Register()
//        {
//            try
//            {
//                InitProtocolAndPort();
//                Register(EndpointHost.MetadataMap.Values.ToArray());
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("Fail to register service!", ex);
//            }
//        }

//        public void Unregister()
//        {
//            try
//            {
//                UnRegister(EndpointHost.MetadataMap.Values.ToArray());
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("Fail to unregister service!", ex);
//            }
//        }

//        private void InitProtocolAndPort()
//        {
//            if (TryGetPortFromProcess(out _port))
//            {
//                _protocol = HTTP_PROTOCOL;
//                _ipAddress = "localhost";
//                return;
//            }

//            Dictionary<string, int> bindings = GetBindings();
//            if (bindings.ContainsKey(HTTP_PROTOCOL))
//            {
//                _protocol = HTTP_PROTOCOL;
//                _port = bindings[HTTP_PROTOCOL];
//                return;
//            }

//            if (bindings.ContainsKey(HTTPS_PROTOCOL))
//            {
//                _protocol = HTTPS_PROTOCOL;
//                _port = bindings[HTTPS_PROTOCOL];
//                return;
//            }

//            _protocol = HTTP_PROTOCOL;
//            _port = 80;
//        }

//        public void Register(string protocol, int port, string virtualPath)
//        {
//            if (string.IsNullOrWhiteSpace(protocol))
//                throw new ArgumentException("Argument \"protocol\" is null");

//            if (port < 0 || port > 65535)
//                throw new ArgumentException("Invalid \"port\" value: " + port);

//            if (string.IsNullOrWhiteSpace(virtualPath))
//                throw new ArgumentException("Argument \"virtualPath\" is null");

//            _protocol = protocol;
//            _port = port;
//            _virtualPath = virtualPath;

//            Register(EndpointHost.MetadataMap.Values.ToArray());
//        }

//        private void Register(params ServiceMetadata[] serviceMetadatas)
//        {
//            Validate();

//            var instances = new List<Instance>();
//            foreach (var serviceMetadata in serviceMetadatas)
//            {
//                var instance = CreateInstance(serviceMetadata);
//                instances.Add(instance);
//                HealthCheckRegistryFilter.Instance.RegisterService(serviceMetadata);
//            }
//            _manager.RegistryClient.Register(instances.ToArray());
//        }

//        private void Validate()
//        {
//            if (string.IsNullOrWhiteSpace(ServiceUtils.HostIP))
//                throw new NotSupportedException("Can not self-register. HostIP is null or white space.");
//        }

//        private void UnRegister(params ServiceMetadata[] serviceMetadatas)
//        {
//            var instances = new List<Instance>();
//            foreach (var serviceMetadata in serviceMetadatas)
//            {
//                var instance = CreateInstance(serviceMetadata);
//                instances.Add(instance);
//            }
//            _manager.RegistryClient.Unregister(instances.ToArray());
//        }

//        private static Dictionary<string, int> GetBindings()
//        {
//            string siteName = HostingEnvironment.SiteName;
//            ConfigurationSection sitesSection = WebConfigurationManager.GetSection(siteName, null, "system.applicationHost/sites");
            
//            var result = new Dictionary<string, int>();
//            foreach (ConfigurationElement site in sitesSection.GetCollection())
//            {
//                if (String.Equals((string)site["name"], siteName, StringComparison.OrdinalIgnoreCase))
//                {
//                    foreach (ConfigurationElement binding in site.GetCollection("bindings"))
//                    {
//                        string protocol = (string)binding["protocol"];
//                        string bindingInfo = (string)binding["bindingInformation"];

//                        if (protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
//                        {
//                            string[] parts = bindingInfo.Split(':');
//                            if (parts.Length == 3)
//                            {
//                                string port = parts[1];
//                                result[protocol] = int.Parse(port);
//                            }
//                        }
//                    }
//                }
//            }

//            return result;
//        }

//        private static bool TryGetPortFromProcess(out int port)
//        {
//            port = 0;
//            try
//            {
//                var process = Process.GetCurrentProcess();
//                if (!process.MainModule.ModuleName.StartsWith("WebDev.WebServer"))
//                    return false;

//                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
//                {
//                    foreach (var managementObject in searcher.Get())
//                    {
//                        string commandLine = managementObject["CommandLine"] as string;
//                        if (string.IsNullOrWhiteSpace(commandLine))
//                            continue;

//                        var result = CommandParser.ParseCommand(commandLine, switches);
//                        int.TryParse(result[PortKey], out port);

//                        if (port != 0)
//                            return true;
//                    }

//                    return false;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Warn("Error occurred while get port from process.", ex);
//                return false;
//            }
//        }
//    }
//}

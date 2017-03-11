// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using System;
using System.Collections.Generic;
using System.Linq;
using AntServiceStack.Common.Consul;
using AntServiceStack.Common.Consul.Discovery;
using AntServiceStack.Common.Consul.Dtos;
using AntServiceStack.Common.Extensions;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.Consul
{
    /// <summary>
    /// Manages register, unregister, holds registration state
    /// </summary>
    public class ConsulDiscovery : IServiceDiscovery<ConsulService, ServiceRegistration>
    {
        /// <summary>
        /// Contains the service registration information
        /// </summary>
        public List<ServiceRegistration> Registration { get; private set; }

        /// <summary>
        /// Registers the apphost with consul
        /// </summary>
        /// <param name="appHost"></param>
        public void Register(IAppHost appHost)
        {
            Registration = new List<ServiceRegistration>();
            // get endpoint http://url:port/path and version

            string baseUrl = string.Empty;

            if (!string.IsNullOrEmpty(appHost.Config.WebHostUrl))
            {
                baseUrl = appHost.Config.WebHostUrl;
            }
            else
            {
                baseUrl = "http://" + (appHost.Config.WebHostIP + (string.IsNullOrEmpty(appHost.Config.WebHostPort) ? ":80" : ":" + appHost.Config.WebHostPort)).CombineWith(appHost.Config.ServiceStackHandlerFactoryPath);
            }
           
            var dtoTypes = GetRequestTypes(appHost);
            var customSetting = appHost.GetPlugin<ConsulFeature>().Settings;
            var customTags = customSetting.GetCustomTags();
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                //可能有多个服务
                //名称用 服务 + IP ？
                // construct registration 
                string host;
                var port = GetPort(baseUrl,out host);
                var registration = new ServiceRegistration
                {
                    Name = metadata.RefinedFullServiceName,
                    Id = $"ss-{host}--{metadata.ServiceName}",
                    Address = baseUrl,
                    Version = 1,
                    Port = port
                };

                // build the service tags
                var tags = new List<string> { $"ss-version-{registration.Version}" };
                tags.AddRange(dtoTypes.Select(x => x.Name));
                tags.AddRange(customTags);
                registration.Tags = tags.ToArray();

                // register the service and healthchecks with consul
                ConsulClient.RegisterService(registration);
                var heathChecks = CreateHealthChecks(registration, customSetting);
                ConsulClient.RegisterHealthChecks(heathChecks);
                registration.HealthChecks = heathChecks;

                // TODO Generate warnings if dto's have [Restrict(RequestAttributes.Secure)] 
                // but are being registered without an https:// baseUri

                // TODO for sorting by versioning to work, any registered version tag must be numeric
                // option 1: use ApiVersion but throw exception to stop host if it is not numeric
                // option 2: use a dedicated numeric version property which defaults to 1.0
                // option 3: use the appost's assembly version
                //var version = "v{0}".Fmt(host.Config?.ApiVersion?.Replace('.', '-'));

                // assign if self-registration was successful
                Registration.Add(registration);
            }
           
        }

        /// <summary>
        /// Unregistered the apphost with consul
        /// </summary>
        /// <param name="appHost"></param>
        public void Unregister(IAppHost appHost)
        {
            if (Registration == null) return;

            foreach (var r in Registration)
            {
                ConsulClient.UnregisterService(r.Id);
            }
            Registration = null;
        }

        public ConsulService[] GetServices(string serviceName)
        {
            var response = ConsulClient.GetServices(serviceName);
            return response.Select(x => new ConsulService(x)).ToArray();
        }

        public ConsulService GetService(string serviceName, string dtoName)
        {
            var response = ConsulClient.GetService(serviceName, dtoName);
            return new ConsulService(response);
        }

        public HashSet<Type> GetRequestTypes(IAppHost host)
        {
            // registered the requestDTO type names for the lookup
            // ignores types based on 
            // https://github.com/ServiceStack/ServiceStack/wiki/Add-ServiceStack-Reference#excluding-types-from-add-servicestack-reference

            return
                host.Config.MetadataMap.SelectMany(r=>r.Value.ServiceTypes)
                    .ToHashSet();
        }

        public string ResolveBaseUri(object dto)
        {
            return ResolveBaseUri(dto.GetType());
        }

        public string ResolveBaseUri(Type dtoType)
        {
            // handles all tag matching, healthy and lowest round trip time (rtt)
            // throws GatewayServiceDiscoveryException back to the Gateway 
            // to allow retry/exception handling at call site
            return ""; //GetService(Registration.Name, dtoType.Name)?.Address;
        }

        private ServiceHealthCheck[] CreateHealthChecks(ServiceRegistration registration, ConsulFeatureSettings settings)
        {
            var checks = new List<ServiceHealthCheck>();
            var serviceId = registration.Id;
            var baseUrl = registration.Address;
            if (settings.IncludeDefaultServiceHealth)
            {
                //注册心跳
                var heartbeatCheck = CreateHeartbeatCheck(baseUrl, serviceId);
                checks.Add(heartbeatCheck);

                //var redisCheck = CreateRedisCheck(serviceId);
                //if (redisCheck != null)
                //    checks.Add(redisCheck);
            }

            //注册healthCheck
            //var customCheck = CreateCustomCheck(baseUrl, serviceId, settings);
            //if (customCheck != null)
            //    checks.Add(customCheck);

            // TODO Setup health checks for any registered IDbConnectionFactories

            return checks.ToArray();
        }
        private int? GetPort(string baseUrl,out string host)
        {
            var uri = new Uri(baseUrl, UriKind.Absolute);
            host = uri.Host;
            return uri.Port;
        }

       

        private ServiceHealthCheck CreateCustomCheck(string baseUrl, string serviceId, ConsulFeatureSettings settings)
        {
            var customHealthCheck = settings.GetHealthCheck();
            if (customHealthCheck == null)
            {
                return null;
            }

            return new ServiceHealthCheck
            {
                Id = "SS-HealthCheck",
                ServiceId = serviceId,
                IntervalInSeconds = customHealthCheck.IntervalInSeconds,
                DeregisterCriticalServiceAfterInMinutes = customHealthCheck.DeregisterIfCriticalAfterInMinutes,
                Http = baseUrl.CombineWith("/json/reply/healthcheck"),
                Notes = "This check is an HTTP GET request which expects the service to return 200 OK"
            };
        }

        //private ServiceHealthCheck CreateRedisCheck(string serviceId)
        //{
        //    var clientsManager = HostContext.TryResolve<IRedisClientsManager>();
        //    if (clientsManager == null)
        //    {
        //        return null;
        //    }

        //    try
        //    {
        //        using (var redisClient = clientsManager.GetReadOnlyClient())
        //        {
        //            if (redisClient != null)
        //            {
        //                var redisHealthCheck = new ServiceHealthCheck
        //                {
        //                    Id = "SS-Redis",
        //                    ServiceId = serviceId,
        //                    IntervalInSeconds = 10,
        //                    Tcp = $"{redisClient.Host}:{redisClient.Port}",
        //                    Notes = "This check ensures that redis is responding correctly"
        //                };
        //                return redisHealthCheck;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.GetLogger(typeof(ConsulClient))
        //            .Error(
        //                "Could not create a redis connection from the registered IRedisClientsManager, skipping consul health check",
        //                ex);
        //    }

        //    return null;
        //}

        private static ServiceHealthCheck CreateHeartbeatCheck(string baseUrl, string serviceId)
        {
            return new ServiceHealthCheck
            {
                Id = "SS-Heartbeat",
                ServiceId = serviceId,
                IntervalInSeconds = 30,
                Http = baseUrl.CombineWith("/json/consul/heartbeat"),
                Notes = "A heartbeat service to check if the service is reachable, expects 200 response",
                DeregisterCriticalServiceAfterInMinutes = 10
            };
        }
    }
}
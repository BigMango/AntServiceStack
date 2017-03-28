using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Client.RegistryClient
{
    internal class DynamicRequestContextProvider
    {
       
        public static ILoadBalancerRequestContext LoadBalancerRequestContext(string serviceKey, string version = null)
        {
            return new ConsulLoadBalancerRequestContext(serviceKey, version);
        }
        public static ILoadBalancerRequestContext LoadSignalRRequestContext(string serviceKey, string version = null)
        {
            return ClientHubClient.CreateInstance().Start(serviceKey);
        }
    }
}

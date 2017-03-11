using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Client.RegistryClient
{
    public class DynamicRequestContextProvider
    {
       
        public static ILoadBalancerRequestContext LoadBalancerRequestContext(string serviceKey, string version = null)
        {
            return new ConsulLoadBalancerRequestContext(serviceKey, version);
        }
       
    }
}

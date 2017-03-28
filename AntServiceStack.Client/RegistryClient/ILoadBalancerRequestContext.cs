using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Consul;

namespace AntServiceStack.Client.RegistryClient
{
    internal interface ILoadBalancerRequestContext
    {
        ConsulServiceResponse[] Servers { get; }

        void MarkServerAvailable();

        void MarkServerUnavailable();
    }
}

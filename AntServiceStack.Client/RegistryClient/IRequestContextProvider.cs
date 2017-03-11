using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Client.RegistryClient
{
    interface IRequestContextProvider
    {
        ILoadBalancerRequestContext LoadBalancerRequestContext(string serviceKey,string url);

        bool NeedCheckAvailable { get; }
    }
}

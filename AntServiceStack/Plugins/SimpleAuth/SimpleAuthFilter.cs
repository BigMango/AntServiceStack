using System;
using System.Collections.Generic;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.Plugins.SimpleAuth
{
    internal class SimpleAuthFilter : IHasRequestFilter
    {
        public int Priority { get; set; }

        public Dictionary<string, List<ISimpleAuthProvider>> AuthProviderMapping { get; private set; }

        public SimpleAuthFilter()
        {
            this.Priority = -1;
            this.AuthProviderMapping = new Dictionary<string, List<ISimpleAuthProvider>>();
        }

        public void RequestFilter(IHttpRequest requset, IHttpResponse response, object requestDto)
        {
            if (String.Equals(requset.OperationName, ServiceUtils.CheckHealthOperationName, StringComparison.OrdinalIgnoreCase)) 
                return;

            List<ISimpleAuthProvider> authProviders;
            if (!this.AuthProviderMapping.TryGetValue(requset.ServicePath, out authProviders))
                return;

            foreach (var authProvider in authProviders)
            {
                if (authProvider.Authenticate(requset, requestDto, requset.OperationName))
                    continue;

                response.ContentType = "text/html";
                response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                response.StatusDescription = "Forbidden by SimpleAuthFilter";
                response.Write("<br/><br/><h1 align='center'>Forbidden (403)</h1>");
                response.EndRequest();
                return;
            }
        }

        public IHasRequestFilter Copy()
        {
            return (IHasRequestFilter)this.MemberwiseClone();
        }
    }
}

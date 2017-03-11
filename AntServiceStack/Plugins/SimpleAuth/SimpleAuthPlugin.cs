using System;
using System.Collections.Generic;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.SimpleAuth
{
    public class SimpleAuthPlugin : IPlugin
    {
        internal static SimpleAuthFilter SimpleAuthFilter { get; private set; }

        static SimpleAuthPlugin()
        {
            SimpleAuthFilter = new SimpleAuthFilter();
        }

        internal SimpleAuthPlugin()
        {

        }

        /// <summary>
        /// 注册认证到根服务
        /// </summary>
        /// <param name="authProviders"></param>
        public static void RegisterSimpleAuthProvider(params ISimpleAuthProvider[] authProviders)
        {
            RegisterSimpleAuthProvider(null, authProviders);
        }

        /// <summary>
        /// 注册认证到指定的服务
        /// </summary>
        /// <param name="authProviders">认证</param>
        /// <param name="servicePath">服务所在的路径</param>
        public static void RegisterSimpleAuthProvider(string servicePath, params ISimpleAuthProvider[] authProviders)
        {
            if (authProviders == null || authProviders.Length == 0)
                return;
            servicePath = String.IsNullOrWhiteSpace(servicePath) ? ServiceMetadata.DefaultServicePath : servicePath.Trim().ToLower();
            if (!EndpointHost.Config.MetadataMap.ContainsKey(servicePath))
                throw new Exception(string.Format("Service path '{0}' is not existing in the AppHost.", servicePath));

            if (!SimpleAuthFilter.AuthProviderMapping.ContainsKey(servicePath))
                SimpleAuthFilter.AuthProviderMapping[servicePath] = new List<ISimpleAuthProvider>();

            SimpleAuthFilter.AuthProviderMapping[servicePath].AddRange(authProviders);
        }

        public void Register(IAppHost appHost)
        {
            if (SimpleAuthFilter.AuthProviderMapping.Count == 0)
                return;
            appHost.RequestFilters.Add(SimpleAuthFilter.RequestFilter);
        }
    }
}

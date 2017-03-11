using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Funq;
using AntServiceStack.Common.Utils;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;

namespace AntServiceStack.WebHost.Endpoints
{
    public static class AppHostExtensions
    {
        private static ILog log = LogManager.GetLogger(typeof(AppHostExtensions));

        public static void RegisterService<TService>(this IAppHost appHost)
        {
            appHost.RegisterService(typeof(TService));
        }

        public static void RegisterRequestBinder<TRequest>(this IAppHost appHost, Func<IHttpRequest, object> binder)
        {
            appHost.RequestBinders[typeof(TRequest)] = binder;
        }

        public static void AddPluginsFromAssembly(this IAppHost appHost, params Assembly[] assembliesWithPlugins)
        {
            foreach (Assembly assembly in assembliesWithPlugins)
            {
                var pluginTypes =
                    from t in assembly.GetExportedTypes()
                    where t.GetInterfaces().Any(x => x == typeof(IPlugin))
                    select t;

                foreach (var pluginType in pluginTypes)
                {
                    try
                    {
                        var plugin = pluginType.CreateInstance() as IPlugin;
                        if (plugin != null)
                        {
                            EndpointHost.AddPlugin(plugin);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Fatal("Error adding new Plugin " + pluginType.Name, ex, new Dictionary<string, string>(){ { "ErrorCode", "FXD300007" } });
                    }
                }
            }
        }
        public static T GetPlugin<T>(this IAppHost appHost) where T : class, IPlugin
        {
            return appHost.Plugins.FirstOrDefault<IPlugin>((Func<IPlugin, bool>)(x => x is T)) as T;
        }
        public static bool HasPlugin<T>(this IAppHost appHost) where T : class, IPlugin
        {
            return appHost.Plugins.FirstOrDefault<IPlugin>((Func<IPlugin, bool>)(x => x is T)) != null;
        }

        public static bool HasMultiplePlugins<T>(this IAppHost appHost) where T : class, IPlugin
        {
            return appHost.Plugins.Count<IPlugin>((Func<IPlugin, bool>)(x => x is T)) > 1;
        }
        /// <summary>
        /// Get an IAppHost container. 
        /// Note: Registering dependencies should only be done during setup/configuration 
        /// stage and remain immutable there after for thread-safety.
        /// </summary>
        /// <param name="appHost"></param>
        /// <returns></returns>
        public static Container GetContainer(this IAppHost appHost)
        {
            var hasContainer = appHost as IHasContainer;
            return hasContainer != null ? hasContainer.Container : null;
        }
    }

}
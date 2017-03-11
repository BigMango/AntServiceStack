using System;
using System.Collections.Generic;
using System.Linq;
using Freeway.Logging;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints;
using System.Reflection;

namespace AntServiceStack.ServiceHost
{
    public class ServiceRoutes : IServiceRoutes
    {
        private static ILog log = LogManager.GetLogger(typeof(ServiceRoutes));

        public readonly List<RestPath> RestPaths = new List<RestPath>();

        public IServiceRoutes Add(MethodInfo mi, string restPath)
        {
            if (HasExistingRoute(mi, restPath)) return this;

            RestPaths.Add(new RestPath(mi, restPath));
            return this;
        }

        public IServiceRoutes Add(MethodInfo mi, string restPath, string verbs)
        {
            if (HasExistingRoute(mi, restPath)) return this;

            RestPaths.Add(new RestPath(mi, restPath, verbs));
            return this;
        }


        public IServiceRoutes Add(MethodInfo mi, string restPath, string verbs, string summary, string notes)
        {
            if (HasExistingRoute(mi, restPath)) return this;

            RestPaths.Add(new RestPath(mi, restPath, verbs, summary, notes));
            return this;
        }

        private bool HasExistingRoute(MethodInfo mi, string restPath)
        {
            var existingRoute = RestPaths.FirstOrDefault(
                x => x.ServiceMethod == mi && x.Path == restPath);

            if (existingRoute != null)
            {
                var existingRouteMsg = "Existing Route for '{0}' at '{1}' already exists".Fmt(mi.Name, restPath);

                //if (!EndpointHostConfig.SkipRouteValidation) //wait till next deployment
                //    throw new Exception(existingRouteMsg);

                log.Warn(existingRouteMsg, 
                    new Dictionary<string, string>() 
                    { 
                        { "ErrorCode", "FXD300048" } 
                    });
                return true;
            }

            return false;
        }
    }
}
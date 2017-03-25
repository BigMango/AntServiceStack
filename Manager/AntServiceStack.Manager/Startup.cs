using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using AntServiceStack.Manager.SignalR;
using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(AntServiceStack.Manager.Startup))]
namespace AntServiceStack.Manager
{
   
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            builder.RegisterHubs(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            builder.RegisterType<DebugLogger>().As<IHubLogger>();
            var container = builder.Build();
            app.UseAutofacMiddleware(container);
            var resolver = new AutofacDependencyResolver(container);
            GlobalHost.HubPipeline.AddModule(new LoggingPipelineModule(resolver.Resolve<IHubLogger>()));
            GlobalHost.HubPipeline.AddModule(new ErrorHandlingPipelineModule(resolver.Resolve<IHubLogger>()));
            app.MapSignalR("/antsoa",new HubConfiguration
            {
                Resolver = resolver,
                EnableJSONP = true,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });

            builder.RegisterInstance(resolver.Resolve<IConnectionManager>());
           
        }
    }
}
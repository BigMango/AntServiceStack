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
           
            //GlobalHost.DependencyResolver.UseRedis("localhost", 6379, string.Empty, "myApp");
            app.MapSignalR("/antsoa",new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });
            
          
           
        }
    }
}
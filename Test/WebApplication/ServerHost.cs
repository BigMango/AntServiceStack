using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.Plugins.Consul;
using AntServiceStack.Plugins.ProtoBuf;
using AntServiceStack.Validation;
using AntServiceStack.WebHost.Endpoints;
using Funq;

namespace WebApplication
{
    public class ServerHost : AppHostBase
    {
        public ServerHost() : base(typeof(SoaController).Assembly) { }
        public override void Configure(Container container)
        {
            //根据域名
            //UpdateConfig(r => r.WebHostUrl, "http://localhost/WebApplication");

            //根据服务器IP

            UpdateConfig(r => r.ServiceStackHandlerFactoryPath, "WebApplication");
            UpdateConfig(r => r.UseConsulDiscovery, false);
            //UpdateConfig(r => r.WebHostPort, "5683");

            Plugins.Add(new ProtoBufFormat());
            Plugins.Add(new ValidationFeature());
           
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntServiceStack.Plugins.Consul;
using AntServiceStack.Plugins.ProtoBuf;
using AntServiceStack.Validation;
using AntServiceStack.WebHost.Endpoints;
using Funq;

namespace MegaApplication
{
    public class ServerHost : AppHostBase
    {
        public ServerHost() : base(typeof(SoaController).Assembly) { }
        public override void Configure(Container container)
        {
            //根据域名
            UpdateConfig(r => r.WebHostUrl, "http://localhost/MegaApplication");

            //根据服务器IP

            //UpdateConfig(r => r.ServiceStackHandlerFactoryPath, "MegaApplication");
            UpdateConfig(r => r.UseConsulDiscovery, true);
            //UpdateConfig(r => r.WebHostPort, "5683");
           
        }
    }
}
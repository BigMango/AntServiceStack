//-----------------------------------------------------------------------
// <copyright file="Host.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using AntServiceStack.Plugins.ProtoBuf;
using AntServiceStack.Validation;
using AntServiceStack.WebHost.Endpoints;
using Funq;

namespace ConsoleServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    /// <summary>
    /// 
    /// </summary>
    public class Host2 : AppHostHttpListenerBase
    {


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Host class.
        /// </summary>
        public Host2() : base(typeof(ConsoleSoaController2).Assembly)
        {
        }



        #endregion
        public override void Configure(Container container)
        {
            //根据域名
            UpdateConfig(r => r.WebHostUrl, "http://192.168.1.2:8089/");

            //根据服务器IP
            
            UpdateConfig(r => r.UseConsulDiscovery, true);
            //UpdateConfig(r => r.WebHostPort, "5683");

            Plugins.Add(new ProtoBufFormat());
            Plugins.Add(new ValidationFeature());
        }
    }
}
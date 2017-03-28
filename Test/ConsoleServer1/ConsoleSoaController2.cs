//-----------------------------------------------------------------------
// <copyright file="ConsoleSoaController.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;
using TestContract.Yuzd;

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
    public class ConsoleSoaController2 : Ihelloyuzd
    {

        public CheckHealthResponseType CheckHealth(CheckHealthRequestType request)
        {
            throw new NotImplementedException();
        }

        [Route("/HelloWorld", Summary = "HelloWorld")]
        public HelloWorldResponseType HelloWorld(HelloWorldRequestType request)
        {
            return new HelloWorldResponseType
            {
                Response = Environment.MachineName
            };

        }

        public Task<HelloWorldResponseType> HelloWorldAsync(HelloWorldRequestType request)
        {
            throw new NotImplementedException();
        }
    }
}
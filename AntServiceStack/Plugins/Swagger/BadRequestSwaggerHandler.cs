//-----------------------------------------------------------------------
// <copyright file="BadRequestSwaggerHandler.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Web;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStackSwagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// 
    /// </summary>
    public class BadRequestSwaggerHandler : IServiceStackHttpHandler, IHttpHandler
    {
        private string _servicePath;
        private string _esbRequestType;
        private string _message;


        public BadRequestSwaggerHandler(string servicePath, string message)
            : this(servicePath, null, message)
        {
        }

        public BadRequestSwaggerHandler(string servicePath, string esbRequestType, string message)
        {
            _servicePath = servicePath;
            _esbRequestType = esbRequestType;
            _message = message;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            string message = string.Format("Swagger Request Data is bad. RequestType: {0}, Cause: {1}", _esbRequestType, _message);
            response.ContentType = "text/plain";
            response.StatusCode = 400;
            response.StatusDescription = "ESB Request Data is bad. RequestType: " + _esbRequestType;
            response.EndHttpHandlerRequest(skipClose: true, afterBody: r => r.Write(message.ToString()));
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var httpReq = new AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper(_servicePath, typeof(NotFoundSwaggerServiceHandler).Name, request);
            var httpRes = new AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper(response);
            HostContext.InitRequest(httpReq, httpRes);
            ProcessRequest(httpReq, httpRes, null);
        }

        public bool IsReusable {
            get { return false; }
        }
    }
}
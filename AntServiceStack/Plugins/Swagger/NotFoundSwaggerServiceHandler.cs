//-----------------------------------------------------------------------
// <copyright file="NotFoundSwaggerServiceHandler.cs" company="Company">
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
    public class NotFoundSwaggerServiceHandler : IServiceStackHttpHandler, IHttpHandler
    {
        private string _servicePath;
        private string _esbRequestType;
        public NotFoundSwaggerServiceHandler(string servicePath, string esbRequestType)
        {
            _servicePath = servicePath;
            _esbRequestType = esbRequestType;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
          
            string message = string.Format("Swagger Request Type is not found. RemoteIP: {0}, ServiceUrl: {1}, RequestType: {2}",
                request.RemoteIp, request.RawUrl, _esbRequestType);

            response.ContentType = "text/plain";
            response.StatusCode = 404;
            response.StatusDescription = "ESB Request Type is not found: " + _esbRequestType;
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

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
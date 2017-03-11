using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;

namespace AntServiceStack.Plugins.OperationInfo
{
    public class OperationInfoHandler : IHttpHandler, IServiceStackHttpHandler
    {
        public const string RestPath = "_operationinfo";

        string _servicePath;

        public OperationInfoHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(_servicePath, typeof(OperationInfoHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(OperationInfoHandler).Name);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (!EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
            {

                var operationList = (from r in EndpointHost.Config.MetadataMap[_servicePath].Operations
                                     let sampleObjects = (ServiceUtils.IsCheckHealthOperation(operationName) ? 
                                                          SampleObjects.CheckHealthSampleMessage : 
                                                          SampleObjects.GetSampleMessage(_servicePath, r.Name)) ?? 
                                                    new SampleMessage(r.RequestType.CreateInstance(), r.ResponseType.CreateInstance())
                                     let requestObject = ReflectionUtils.PopulateObject(sampleObjects.Request)
                                     let responseObject = ReflectionUtils.PopulateObject(sampleObjects.Response)
                                     select new
                                     {
                                         Name = r.Name,
                                         RequestMessage = new
                                         {
                                             Xml = XmlSerializeToString(requestObject),
                                             Json = WrappedJsonSerializer.Instance.SerializeToString(requestObject),
                                         },
                                         ResponseMessage = new 
                                         {
                                             Xml = XmlSerializeToString(responseObject),
                                             Json = WrappedJsonSerializer.Instance.SerializeToString(responseObject),
                                         }
                                     }).OrderBy(item => item.Name).ToList();

                httpRes.ContentType = "application/json";
                httpRes.Write(WrappedJsonSerializer.Instance.SerializeToString(operationList));
            }
        }

        private string XmlSerializeToString(object obj)
        {
            try
            {
                return WrappedXmlSerializer.SerializeToString(obj, true);
            }
            catch(Exception)
            {
                return "";
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}

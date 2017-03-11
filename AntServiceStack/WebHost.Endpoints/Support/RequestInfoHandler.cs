using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    [XmlType]
    public class RequestInfo { }

    [XmlType]
    public class RequestInfoResponse
    {
        [XmlElement]
        public string Host { get; set; }

        [XmlElement]
        public DateTime Date { get; set; }

        [XmlElement]
        public string ServiceName { get; set; }

        [XmlElement]
        public string HandlerPath { get; set; }

        [XmlElement]
        public string UserHostAddress { get; set; }

        [XmlElement]
        public string HttpMethod { get; set; }

        [XmlElement]
        public string PathInfo { get; set; }

        [XmlElement]
        public string ResolvedPathInfo { get; set; }

        [XmlElement]
        public string Path { get; set; }

        [XmlElement]
        public string AbsoluteUri { get; set; }

        [XmlElement]
        public string ApplicationPath { get; set; }

        [XmlElement]
        public string HandlerFactoryArgs { get; set; }

        [XmlElement]
        public string RawUrl { get; set; }

        [XmlElement]
        public string Url { get; set; }

        [XmlElement]
        public string ContentType { get; set; }

        [XmlElement]
        public int Status { get; set; }

        [XmlElement]
        public long ContentLength { get; set; }

        [XmlArray]
        public List<KeyValuePair> Headers { get; set; }

        [XmlArray]
        public List<KeyValuePair> QueryString { get; set; }

        [XmlArray]
        public List<KeyValuePair> FormData { get; set; }

        [XmlArray]
        public List<string> AcceptTypes { get; set; }

        [XmlElement]
        public string ServicePath { get; set; }

        [XmlElement]
        public string OperationName { get; set; }

        [XmlElement]
        public string ResponseContentType { get; set; }

        [XmlElement]
        public string ErrorCode { get; set; }

        [XmlElement]
        public string ErrorMessage { get; set; }

        [XmlElement]
        public string DebugString { get; set; }

        [XmlArray]
        public List<string> OperationNames { get; set; }

        [XmlArray]
        public List<string> AllOperationNames { get; set; }

        [XmlArray]
        public List<KeyValuePair> RequestResponseMap { get; set; }
    }

    [XmlType]
    public class KeyValuePair
    {
        [XmlElement]
        public string Key { get; set; }

        [XmlElement]
        public string Value { get; set; }
    }

    public class RequestInfoHandler
        : IServiceStackHttpHandler, IHttpHandler
    {
        public const string RestPath = "_requestinfo";

        private string _servicePath;

        public RequestInfoResponse RequestInfo { get; set; }

        public RequestInfoHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            var response = this.RequestInfo ?? GetRequestInfo(httpReq);
            response.HandlerFactoryArgs = AntServiceStackHttpHandlerFactory.DebugLastHandlerArgs;
            response.DebugString = "";
            if (HttpContext.Current != null)
            {
                response.DebugString += HttpContext.Current.Request.GetType().FullName
                    + "|" + HttpContext.Current.Response.GetType().FullName;
            }

            var json = JsonSerializer.SerializeToString(response);
            httpRes.ContentType = ContentType.Json;
            httpRes.Write(json);
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new HttpRequestWrapper(_servicePath, typeof(RequestInfo).Name, context.Request);
            IHttpResponse response = new HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(RequestInfo).Name);
        }

        public static List<KeyValuePair> ToKeyValuePairList(NameValueCollection nvc)
        {
            var list = new List<KeyValuePair>();
            for (var i = 0; i < nvc.Count; i++)
            {
                list.Add(new KeyValuePair() { Key = nvc.GetKey(i), Value = nvc.Get(i) });
            }
            return list;
        }

        public static Dictionary<string, string> ToDictionary(NameValueCollection nvc)
        {
            var map = new Dictionary<string, string>();
            for (var i = 0; i < nvc.Count; i++)
            {
                map[nvc.GetKey(i)] = nvc.Get(i);
            }
            return map;
        }

        public static string ToString(NameValueCollection nvc)
        {
            var map = ToDictionary(nvc);
            return TypeSerializer.SerializeToString(map);
        }

        public static RequestInfoResponse GetRequestInfo(IHttpRequest httpReq)
        {
            ServiceMetadata serviceMetadata = EndpointHost.Config.MetadataMap[httpReq.ServicePath];
            var response = new RequestInfoResponse
            {
                Host = EndpointHost.Config.DebugHttpListenerHostEnvironment + "_v" + Env.AntServiceStackVersion + "_" + serviceMetadata.FullServiceName,
                Date = DateTime.UtcNow,
                ServiceName = serviceMetadata.FullServiceName,
                UserHostAddress = httpReq.UserHostAddress,
                HttpMethod = httpReq.HttpMethod,
                AbsoluteUri = httpReq.AbsoluteUri,
                RawUrl = httpReq.RawUrl,
                ResolvedPathInfo = httpReq.PathInfo,
                ContentType = httpReq.ContentType,
                Headers = ToKeyValuePairList(httpReq.Headers),
                QueryString = ToKeyValuePairList(httpReq.QueryString),
                FormData = ToKeyValuePairList(httpReq.FormData),
                AcceptTypes = new List<string>(httpReq.AcceptTypes ?? new string[0]),
                ContentLength = httpReq.ContentLength,
                ServicePath = httpReq.ServicePath,
                OperationName = httpReq.OperationName,
                ResponseContentType = httpReq.ResponseContentType,
            };
            return response;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
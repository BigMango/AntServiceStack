using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Web;
using System.Net;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Soap11;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using HttpRequestExtensions = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestExtensions;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;
using System.Threading.Tasks;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.WebHost.Endpoints
{
    public class Soap11Handler : EndpointHandlerBase
    {
        public Soap11Handler(string servicePath)
            : base(servicePath)
        {
            this.HandlerContentType = ContentType.Soap11;
            this.ContentTypeAttribute = ContentType.GetEndpointAttributes(ContentType.Soap11);
            this.HandlerAttributes = EndpointAttributes.Reply | EndpointAttributes.Soap11 | EndpointAttributes.HttpPost;
            this.format = Feature.Soap11;
        }

        static readonly byte[] SOAP_ENVELOPE_PREFIX = @"<?xml version=""1.0"" encoding=""utf-8""?><soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body>".ToUtf8Bytes();
        static readonly byte[] SOAP_ENVELOPE_SUFFIX = @"</soap:Body></soap:Envelope>".ToUtf8Bytes();

        // for response wrapped in soap envelope, xml declaration is not needed
        static ResponseSerializerDelegate NO_XML_DECL_SERIALIZER = (httpRequest, dto, httpResponse) => WrappedXmlSerializer.SerializeToStreamWithoutXmlDeclaration(dto, httpResponse.OutputStream);

        private Feature format;
        public string HandlerContentType { get; set; }

        public EndpointAttributes ContentTypeAttribute { get; set; }

        public override object CreateRequest(IHttpRequest request, string operationName)
        {
            return GetRequest(request, operationName);
        }

        public object GetRequest(IHttpRequest httpReq, string operationName)
        {
            var requestType = GetRequestType(operationName, httpReq.ServicePath);
            AssertOperationExists(operationName, requestType);

            if (httpReq.ContentLength <= 0)
            {
                throw new ArgumentNullException("Missing request body");
            }

            try
            {
                // Deserialize to soap11 envelope
                var soap11Envelope = WrappedXmlSerializer.DeserializeFromStream<Envelope>(httpReq.InputStream);

                // check existence of the request xml element
                if (soap11Envelope != null && soap11Envelope.Body != null && soap11Envelope.Body.Any.Count > 0)
                {
                    // Get request xml element
                    var requestXmlElement = soap11Envelope.Body.Any[0];
                    if (requestXmlElement != null)
                    {
                        // Get request xml
                        var requestXml = requestXmlElement.OuterXml;
                        if (!string.IsNullOrEmpty(requestXml))
                        {
                            // Deserialize to object of request type
                            return WrappedXmlSerializer.DeserializeFromString(requestXml, requestType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Could not deserialize soap11 request into instance of {0}'\nError: {1}"
                    .Fmt(requestType, ex);
                throw new SerializationException(msg);
            }


            throw new ArgumentNullException("Invalid soap11 request message");
        }


        public override object GetResponse(IHttpRequest httpReq, IHttpResponse httpRes, object request)
        {
            var response = ExecuteService(request,
                HandlerAttributes | httpReq.GetAttributes(), httpReq, httpRes);

            return response;
        }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            try
            {
                EndpointHost.Config.AssertFeatures(format);

                if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes)) return;

                httpReq.ResponseContentType = this.HandlerContentType;

                if (httpReq.HttpMethod != HttpMethods.Post)
                {
                    throw new NotSupportedException("SOAP handler only supports HTTP Post method");
                }

                var request = CreateRequest(httpReq, operationName);
                httpReq.RequestObject = request;
                if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName)) return;

                var response = GetResponse(httpReq, httpRes, request);
                if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, response, operationName)) return;

                httpRes.WriteToResponse(httpReq, response, NO_XML_DECL_SERIALIZER, SOAP_ENVELOPE_PREFIX, SOAP_ENVELOPE_SUFFIX);
            }
            catch (Exception ex)
            {
                if (!EndpointHost.Config.WriteErrorsToResponse) throw;
                HandleException(httpReq, httpRes, operationName, ex);
            }
            finally
            {
                if (EndpointHost.PostResponseFilters.Count > 0)
                {
                    EndpointHost.ApplyPostResponseFilters(new PostResponseFilterArgs()
                    {
                        ExecutionResult = httpRes.ExecutionResult,
                        ServicePath = httpReq.ServicePath,
                        OperationName = httpReq.OperationName,
                        RequestDeserializeTimeInMilliseconds = httpReq.DeserializationTimeInMillis,
                        ResponseSerializeTimeInMilliseconds = httpRes.SerializationTimeInMillis
                    });
                }
            }
        }

        protected override bool PreProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, out object requestObject)
        {
            requestObject = null;

            EndpointHost.Config.AssertFeatures(format);

            if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                return true;

            httpReq.ResponseContentType = this.HandlerContentType;

            if (httpReq.HttpMethod != HttpMethods.Post)
            {
                throw new NotSupportedException("SOAP handler only supports HTTP Post method");
            }

            var request = CreateRequest(httpReq, operationName);
            httpReq.RequestObject = requestObject = request;
            if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName))
                return true;

            return false;
        }

        protected override bool PostProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, object responseObject)
        {
            if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, responseObject, operationName))
                return true;

            httpRes.WriteToResponse(httpReq, responseObject, NO_XML_DECL_SERIALIZER, SOAP_ENVELOPE_PREFIX, SOAP_ENVELOPE_SUFFIX);
            return true;
        }
    }
}

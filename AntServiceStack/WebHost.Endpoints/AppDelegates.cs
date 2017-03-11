using System;
using System.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints
{
    public delegate IHttpHandler HttpHandlerResolverDelegate(string httpMethod, string servicePath, string pathInfo, string filePath);

    public delegate bool StreamSerializerResolverDelegate(IRequestContext requestContext, object dto, IHttpResponse httpRes);

    public delegate void HandleUncaughtExceptionDelegate(IHttpRequest httpReq, IHttpResponse httpRes, Exception ex);

    public delegate object HandleServiceExceptionDelegate(object request, Exception ex, Type reponseType);

    public delegate RestPath FallbackRestPathDelegate(string httpMethod, string servicePath, string pathInfo, string filePath);
}
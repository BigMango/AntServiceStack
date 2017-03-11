using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Web;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;
using AntServiceStack.ServiceHost;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    using System.Text;
    using ServiceHost;

    public abstract class BaseMetadataHandler : HttpHandlerBase, IServiceStackHttpHandler
    {
        public enum DtoType
        {
            Request,
            Response
        }

        public abstract Format Format { get; }

        public string ContentType { get; set; }
        public string ContentFormat { get; set; }

        public readonly string ServicePath;

        public BaseMetadataHandler(string servicePath)
        {
            ServicePath = servicePath;
        }

        public override void Execute(HttpContext context)
        {
            var writer = new HtmlTextWriter(context.Response.Output);
            context.Response.ContentType = "text/html";

            IHttpRequest request = new HttpRequestWrapper(ServicePath, GetType().Name, context.Request);
            IHttpResponse response = new HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);

            try
            {
                if (!EndpointHost.MetadataMap[request.ServicePath].MetadataFeatureEnabled)
                {
                    new NotFoundHttpHandler(request.ServicePath).ProcessRequest(request, response, request.OperationName);
                    return;
                }

                ProcessOperations(writer, request, response);
            }
            catch (Exception ex)
            {
                WriteException(writer, ex);
            }
        }

        public virtual void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            using (var sw = new StreamWriter(httpRes.OutputStream))
            {
                var writer = new HtmlTextWriter(sw);
                httpRes.ContentType = "text/html";
                try
                {
                    ProcessOperations(writer, httpReq, httpRes);
                }
                catch (Exception ex)
                {
                    WriteException(writer, ex);
                }
            }
        }

        public virtual string CreateRequestOrResponse(Type type)
        {
            if (type == typeof(string))
                return "(string)";
            if (type == typeof(byte[]))
                return "(byte[])";
            if (type == typeof(Stream))
                return "(Stream)";
            if (type == typeof(HttpWebResponse))
                return "(HttpWebResponse)";

            return CreateMessage(type);
        }

        public virtual string CreateRequestOrResponse(Type type, string operation, DtoType dtoType)
        {
            if (type == typeof(string))
                return "(string)";
            if (type == typeof(byte[]))
                return "(byte[])";
            if (type == typeof(Stream))
                return "(Stream)";
            if (type == typeof(HttpWebResponse))
                return "(HttpWebResponse)";

            return CreateMessage(type, operation, dtoType);
        }

        protected virtual void WriteException(HtmlTextWriter writer, Exception ex)
        {
            try
            {
                bool isInner = false;
                while (ex != null)
                {
                    string message = string.Format("<br/>{0}Exception: {1}<br/>StackTrace: <br/>{2}<br/>",
                        isInner ? "Inner " : null, ex.Message, ex.StackTrace);
                    writer.Write(message);
                    ex = ex.InnerException;
                    isInner = true;
                }
            }
            catch
            {
            }
        }

        protected virtual void ProcessOperations(HtmlTextWriter writer, IHttpRequest httpReq, IHttpResponse httpRes)
        {
            var operationName = httpReq.QueryString["op"];

            if (!AssertAccess(httpReq, httpRes, operationName)) return;

            ContentFormat = Common.Web.ContentType.GetContentFormat(Format);
            var metadata = EndpointHost.Config.MetadataMap[httpReq.ServicePath];
            if (operationName != null)
            {
                var op = metadata.GetOperationByOpName(operationName);

                var requestType = metadata.GetRequestTypeByOpName(operationName);
                var requestMessage = CreateRequestOrResponse(requestType, operationName, DtoType.Request);
                httpReq.RequestObject = requestMessage;
                string responseMessage = null;

                var responseType = metadata.GetResponseTypeByOpName(operationName);
                if (responseType != null)
                {
                    responseMessage = CreateRequestOrResponse(responseType, operationName, DtoType.Response);
                }

                var isSoap = Format == Format.Soap11 || Format == Format.Soap12;
                var sb = new StringBuilder();
                var description = op.Descritpion;
                if (!description.IsNullOrEmpty())
                {
                    sb.AppendFormat("<h3 id='desc'>{0}</div>", ConvertToHtml(description));
                }

                if (op.Routes.Count > 0)
                {
                    sb.Append("<table>");
                    if (!isSoap)
                    {
                        sb.Append("<caption>The following routes are available for this service:</caption>");
                    }
                    sb.Append("<tbody>");

                    foreach (var route in op.Routes)
                    {
                        if (isSoap && !(route.AllowsAllVerbs || route.AllowedVerbs.Contains(HttpMethods.Post)))
                            continue;

                        sb.Append("<tr>");
                        var verbs = route.AllowsAllVerbs ? "All Verbs" : route.AllowedVerbs;

                        if (!isSoap)
                        {
                            var path = "/" + PathUtils.CombinePaths(EndpointHost.Config.ServiceStackHandlerFactoryPath, route.Path);

                            sb.AppendFormat("<th>{0}</th>", verbs);
                            sb.AppendFormat("<th>{0}</th>", path);
                        }
                        sb.AppendFormat("<td>{0}</td>", route.Summary);
                        sb.AppendFormat("<td><i>{0}</i></td>", route.Notes);
                        sb.Append("</tr>");
                    }

                    sb.Append("<tbody>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                }

                var apiMembers = requestType.GetApiMembers();
                if (apiMembers.Count > 0)
                {
                    sb.Append("<table><caption>Parameters:</caption>");
                    sb.Append("<thead><tr>");
                    sb.Append("<th>Name</th>");
                    sb.Append("<th>Parameter</th>");
                    sb.Append("<th>Data Type</th>");
                    sb.Append("<th>Required</th>");
                    sb.Append("<th>Description</th>");
                    sb.Append("</tr></thead>");

                    sb.Append("<tbody>");
                    foreach (var apiMember in apiMembers)
                    {
                        sb.Append("<tr>");
                        sb.AppendFormat("<td>{0}</td>", ConvertToHtml(apiMember.Name));
                        sb.AppendFormat("<td>{0}</td>", apiMember.ParameterType);
                        sb.AppendFormat("<td>{0}</td>", apiMember.DataType);
                        sb.AppendFormat("<td>{0}</td>", apiMember.IsRequired ? "Yes" : "No");
                        sb.AppendFormat("<td>{0}</td>", apiMember.Description);
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                }

                sb.Append(@"<div class=""call-info"">");
                var overrideExtCopy = EndpointHost.Config.AllowRouteContentTypeExtensions
                   ? " the <b>.{0}</b> suffix or ".Fmt(ContentFormat)
                   : "";
                if (!isSoap)
                {
                    sb.AppendFormat(@"<p>To override the Content-type in your clients HTTP <b>Accept</b> Header, append {1} <b>?format={0}</b></p>", ContentFormat, overrideExtCopy);
                }
                if (ContentFormat == "json")
                {
                    sb.Append("<p>To embed the response in a <b>jsonp</b> callback, append <b>?callback=myCallback</b></p>");
                }
                sb.Append("</div>");

                RenderOperation(writer, httpReq, operationName, requestMessage, responseMessage, sb.ToString());
                return;
            }

            RenderOperations(writer, httpReq, metadata);
        }

        private string ConvertToHtml(string text)
        {
            return text.Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\n", "<br />\n");
        }

        protected bool AssertAccess(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (!EndpointHost.Config.HasAccessToMetadata(httpReq, httpRes)) return false;

            if (operationName == null) return true; //For non-operation pages we don't need to check further permissions
            if (!EndpointHost.Config.EnableAccessRestrictions) return true;
            if (!EndpointHost.Config.CreateMetadataPagesConfig(httpReq.ServicePath).IsVisible(httpReq, Format, operationName))
            {
                EndpointHost.Config.HandleErrorResponse(httpReq, httpRes, HttpStatusCode.Forbidden, "Service Not Available");
                return false;
            }

            return true;
        }

        protected virtual string CreateMessage(Type type, string operatinoName, DtoType dtoType)
        {
            return CreateMessage(type);
        }

        protected abstract string CreateMessage(Type type);

        protected virtual void RenderOperation(HtmlTextWriter writer, IHttpRequest httpReq, string operationName,
            string requestMessage, string responseMessage, string metadataHtml)
        {
            var operationControl = new OperationControl
            {
                HttpRequest = httpReq,
                MetadataConfig = EndpointHost.Config.ServiceEndpointsMetadataConfig,
                Title = EndpointHost.Config.MetadataMap[httpReq.ServicePath].FullServiceName,
                Format = this.Format,
                OperationName = operationName,
                HostName = httpReq.GetUrlHostName(),
                RequestMessage = requestMessage,
                ResponseMessage = responseMessage,
                MetadataHtml = metadataHtml,
            };
            if (!this.ContentType.IsNullOrEmpty())
            {
                operationControl.ContentType = this.ContentType;
            }
            if (!this.ContentFormat.IsNullOrEmpty())
            {
                operationControl.ContentFormat = this.ContentFormat;
            }

            operationControl.Render(writer);
        }

        protected abstract void RenderOperations(HtmlTextWriter writer, IHttpRequest httpReq, ServiceMetadata metadata);

    }
}
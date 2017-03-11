using System.Web;

namespace AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls
{
    internal class Soap11OperationControl : OperationControl
    {
        public Soap11OperationControl()
        {
            Format = ServiceHost.Format.Soap11;
        }

        public override string RequestUri
        {
            get
            {
                var endpointConfig = MetadataConfig.Soap11;
                var endpontPath = ResponseMessage != null ? endpointConfig.SyncReplyUri : endpointConfig.AsyncOneWayUri;
                return endpontPath + "/" + OperationName;
            }
        }

        public override string HttpRequestTemplate
        {
            get
            {
                return string.Format(
@"POST {0} HTTP/1.1 
Host: {1} 
Content-Type: text/xml; charset=utf-8
Content-Length: <span class=""value"">length</span>

{2}", RequestUri, HostName, HttpUtility.HtmlEncode(RequestMessage));
            }
        }

    }
}
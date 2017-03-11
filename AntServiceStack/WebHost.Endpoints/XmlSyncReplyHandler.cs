using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints
{
    public class XmlSyncReplyHandler : GenericHandler
    {
        public XmlSyncReplyHandler(string servicePath)
            : base(servicePath, ContentType.Xml, EndpointAttributes.Reply | EndpointAttributes.Xml, Feature.Xml)
        {
        }
    }
}
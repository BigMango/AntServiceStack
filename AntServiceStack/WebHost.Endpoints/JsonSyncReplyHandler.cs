using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints
{
    public class JsonSyncReplyHandler : GenericHandler
    {
        public JsonSyncReplyHandler(string servicePath)
            : base(servicePath, ContentType.Json, EndpointAttributes.Reply | EndpointAttributes.Json, Feature.Json)
        {
        }
    }
}
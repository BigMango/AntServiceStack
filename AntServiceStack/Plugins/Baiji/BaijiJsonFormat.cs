using System;
using System.IO;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.Plugins.Baiji
{
    internal class BaijiJsonFormat : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.ContentTypeFilters.Register(ContentType.BaijiJson, WrappedBaijiJsonSerializer.Serialize, WrappedBaijiJsonSerializer.Deserialize);
        }
    }
}

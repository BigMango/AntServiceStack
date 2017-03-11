using System;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public class IndexMetadataHandler : BaseSoapMetadataHandler
    {
        public IndexMetadataHandler(string servicePath)
            : base(servicePath)
        {
        }

        public override Format Format { get { return Format.Soap12; } }

        protected override string CreateMessage(Type dtoType)
        {
            return null;
        }
    }
}
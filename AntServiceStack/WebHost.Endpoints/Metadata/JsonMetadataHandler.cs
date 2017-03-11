using System;
using System.Web.UI;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public class JsonMetadataHandler : BaseMetadataHandler
    {
        public override Format Format { get { return Format.Json; } }

        public JsonMetadataHandler(string servicePath)
            : base(servicePath)
        {
        }

        protected override string CreateMessage(Type dtoType)
        {
            var requestObj = ReflectionUtils.PopulateObject(dtoType.CreateInstance());
            return WrappedJsonSerializer.Instance.SerializeToString(requestObj);
        }

        protected override void RenderOperations(HtmlTextWriter writer, IHttpRequest httpReq, ServiceMetadata metadata)
        {
            var defaultPage = new OperationsControl
            {
                Title = metadata.FullServiceName,
                OperationNames = metadata.GetAllOperationNames(),
            };

            defaultPage.RenderControl(writer);
        }
    }
}
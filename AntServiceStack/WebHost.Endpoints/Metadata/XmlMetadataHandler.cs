using System;
using System.Web.UI;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;
using AntServiceStack.Text;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public class XmlMetadataHandler : BaseMetadataHandler
    {
        public override Format Format { get { return Format.Xml; } }

        public XmlMetadataHandler(string servicePath)
            : base(servicePath)
        {
        }

        protected override string CreateMessage(Type dtoType)
        {
            var requestObj = ReflectionUtils.PopulateObject(dtoType.CreateInstance());
            return WrappedXmlSerializer.SerializeToString(requestObj, true);
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
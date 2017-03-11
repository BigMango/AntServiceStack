using System;
using System.Web.UI;
using AntServiceStack.Text;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Extensions;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;
using AntServiceStack.WebHost.Endpoints.Metadata;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public class Soap11MetadataHandler : BaseMetadataHandler
    {
        public override Format Format { get { return Format.Soap11; } }

        public Soap11MetadataHandler(string servicePath)
            : base(servicePath)
        {
        }

        protected override string CreateMessage(Type dtoType)
        {
            var requestObj = ReflectionUtils.PopulateObject(Activator.CreateInstance(dtoType));
            var xml = WrappedXmlSerializer.SerializeToStringWithoutXmlDeclaration(requestObj, true);
            var soapEnvelope = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>

{0}

    </soap:Body>
</soap:Envelope>", xml);
            return soapEnvelope;
        }

        protected override void RenderOperation(System.Web.UI.HtmlTextWriter writer, IHttpRequest httpReq, string operationName, string requestMessage, string responseMessage, string metadataHtml)
        {
            var operationControl = new Soap11OperationControl
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

        protected override void RenderOperations(HtmlTextWriter writer, IHttpRequest httpReq, ServiceMetadata metadata)
        {
            var defaultPage = new IndexOperationsControl
            {
                HttpRequest = httpReq,
                MetadataConfig = EndpointHost.Config.CreateMetadataPagesConfig(metadata),
                Title = metadata.FullServiceName,
                AntServiceStackVersion = metadata.AntServiceStackVersion,
                CCodeGenVersion = metadata.CCodeGenVersion,
                OperationNames = metadata.GetAllOperationNames()
            };

            defaultPage.RenderControl(writer);
        }
    }
}
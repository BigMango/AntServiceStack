using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Collections.Generic;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Web;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public class CustomMetadataHandler
        : BaseMetadataHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CustomMetadataHandler));

        public CustomMetadataHandler(string servicePath, string contentType, string format)
            : base(servicePath)
        {
            base.ContentType = contentType;
            base.ContentFormat = format;
        }

        public override Format Format
        {
            get { return base.ContentFormat.ToFormat(); }
        }

        protected override string CreateMessage(Type dtoType)
        {
            try
            {
                var requestObj = ReflectionUtils.PopulateObject(Activator.CreateInstance(dtoType));

                using (var ms = new MemoryStream())
                {
                    EndpointHost.ContentTypeFilter.SerializeToStream(
                        new SerializationContext(this.ContentType), requestObj, ms);

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                var error = string.Format("Error serializing type '{0}' with custom format '{1}'",
                    dtoType.Name, this.ContentFormat);
                Log.Error(error, ex, new Dictionary<string, string>(){ { "ErrorCode", "FXD300010" } });

                return string.Format("{{Unable to show example output for type '{0}' using the custom '{1}' filter}}" + ex.Message,
                    dtoType.Name, this.ContentFormat);
            }
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
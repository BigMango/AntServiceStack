using AntServiceStack.Common.Web;

namespace AntServiceStack.WebHost.Endpoints.Metadata.Config
{
    public class ServiceEndpointsMetadataConfig
    {
        /// <summary>
        /// Changes the links for the AntServiceStack/metadata page
        /// </summary>
        /// <param name="serviceStackHandlerPrefix"></param>
        /// <returns></returns>
        public static ServiceEndpointsMetadataConfig Create(string serviceStackHandlerPrefix)
        {
            var config = new MetadataConfig("{0}", "{0}", "/{0}", "/{0}", "/{0}/metadata");
            return new ServiceEndpointsMetadataConfig
            {
                DefaultMetadataUri = "/metadata",
                Soap11 = new MetadataConfig("soap11", "SOAP 1.1", "/soap11", "/soap11", "/soap11/metadata"),
                Xml = config.Create("xml"),
                Json = config.Create("json"),
                Jsv = config.Create("jsv"),
                Custom = config
            };
        }

        public string DefaultMetadataUri { get; set; }
        public MetadataConfig Soap11 { get; set; }
        public MetadataConfig Xml { get; set; }
        public MetadataConfig Json { get; set; }
        public MetadataConfig Jsv { get; set; }
        public MetadataConfig Custom { get; set; }

        public MetadataConfig GetEndpointConfig(string contentType)
        {
            switch (contentType)
            {
                case ContentType.Soap11:
                    return this.Soap11;
                case ContentType.Xml:
                    return this.Xml;
                case ContentType.Json:
                    return this.Json;
                case ContentType.Jsv:
                    return this.Jsv;
            }

            var format = ContentType.GetContentFormat(contentType);
            return Custom.Create(format);
        }
    }
}
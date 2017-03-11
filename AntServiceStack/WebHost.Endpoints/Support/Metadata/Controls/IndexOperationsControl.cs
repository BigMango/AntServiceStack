using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Support.Templates;

namespace AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls
{
    internal class IndexOperationsControl : System.Web.UI.Control
    {
        public IHttpRequest HttpRequest { get; set; }
        public string Title { get; set; }
        public string AntServiceStackVersion { get; set; }
        public string CCodeGenVersion { get; set; }
        public List<string> OperationNames { get; set; }
        public MetadataPagesConfig MetadataConfig { get; set; }

        public string RenderRow(string operation)
        {
            var show = EndpointHost.DebugMode; //Always show in DebugMode

            var parentPath = HttpRequest.GetParentAbsolutePath();
            var opTemplate = new StringBuilder("<tr><th>{0}</th>");
            foreach (var config in MetadataConfig.AvailableFormatConfigs)
            {
                var uri = parentPath + config.DefaultMetadataUri;
                if (MetadataConfig.IsVisible(HttpRequest, config.Format.ToFormat(), operation))
                {
                    show = true;
                    opTemplate.AppendFormat(@"<td><a href=""{0}?op={{0}}"">{1}</a></td>", uri, config.Name);
                }
                else
                    opTemplate.AppendFormat("<td>{0}</td>", config.Name);
            }

            opTemplate.Append("</tr>");

            return show ? string.Format(opTemplate.ToString(), operation) : "";
        }

        protected override void Render(HtmlTextWriter output)
        {
            var operationsPart = new TableTemplate
            {
                Title = "Operations:",
                Items = this.OperationNames,
                ForEachItem = RenderRow
            }.ToString();

            var debugOnlyInfo = new StringBuilder();
            if (EndpointHost.DebugMode)
            {
                debugOnlyInfo.Append("<h3>Debug Info:</h3>");
                debugOnlyInfo.AppendLine("<ul>");
                debugOnlyInfo.AppendLine("<li><a href=\"operations/metadata\">Operations Metadata</a></li>");
                debugOnlyInfo.AppendLine("</ul>");
            }

            var renderedTemplate = string.Format(
                HtmlTemplates.IndexOperationsTemplate,
                this.Title,
                operationsPart,
                debugOnlyInfo,
                AntServiceStackVersion,
                CCodeGenVersion);

            output.Write(renderedTemplate);
        }

    }
}
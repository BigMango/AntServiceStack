using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Support.Templates;

namespace AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls
{
    internal class IndexServicesControl : System.Web.UI.Control
    {
        public IHttpRequest HttpRequest { get; set; }
        public string AntServiceStackVersion { get; set; }
        public Dictionary<string, string> ServiceData { get; set; }

        public string RenderRow(string servicePath)
        {
            return string.Format(
                @"<tr><th>{0}</th><td><a href=""{1}"">{2}</a></td></tr>",
                ServiceData[servicePath],
                (HttpRequest.AbsoluteUri.WithTrailingSlash() + servicePath).WithTrailingSlash() + "metadata",
                string.IsNullOrWhiteSpace(servicePath) ? "[root]" : servicePath);
        }

        protected override void Render(HtmlTextWriter output)
        {
            var servicesPart = new TableTemplate
            {
                Title = "Services:",
                Items = ServiceData.Keys.ToList(),
                ForEachItem = RenderRow
            }.ToString();

            var renderedTemplate = string.Format(
                HtmlTemplates.IndexServicesTemplate,
                servicesPart,
                AntServiceStackVersion);

            output.Write(renderedTemplate);
        }
    }
}
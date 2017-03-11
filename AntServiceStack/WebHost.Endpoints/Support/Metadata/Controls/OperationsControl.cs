using System.Collections.Generic;
using System.Web.UI;
using AntServiceStack.WebHost.Endpoints.Support.Templates;

namespace AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls
{
    public class OperationsControl : System.Web.UI.Control
    {
        public string Title { get; set; }
        public List<string> OperationNames { get; set; }

        protected override void Render(HtmlTextWriter output)
        {
            var operationsPart = new ListTemplate
            {
                ListItems = this.OperationNames,
                ListItemTemplate = @"<li><a href=""?op={0}"">{0}</a></li>"
            }.ToString();
            var renderedTemplate = string.Format(HtmlTemplates.OperationsControlTemplate,
                this.Title, operationsPart);
            output.Write(renderedTemplate);
        }

    }
}
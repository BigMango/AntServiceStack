using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace AntServiceStack.WebHost.Endpoints.Support.Templates
{
    public static class HtmlTemplates
    {
        public static string IndexServicesTemplate;
        public static string IndexOperationsTemplate;
        public static string OperationControlTemplate;
        public static string OperationsControlTemplate;

        static HtmlTemplates()
        {
            IndexServicesTemplate = LoadEmbeddedHtmlTemplate("IndexServices.html");
            IndexOperationsTemplate = LoadEmbeddedHtmlTemplate("IndexOperations.html");
            OperationControlTemplate = LoadEmbeddedHtmlTemplate("OperationControl.html");
            OperationsControlTemplate = LoadEmbeddedHtmlTemplate("OperationsControl.html");
        }

        private static string LoadEmbeddedHtmlTemplate(string templateName)
        {
            string _resourceNamespace = typeof(HtmlTemplates).Namespace + ".Html.";
            var stream = typeof(HtmlTemplates).Assembly.GetManifestResourceStream(_resourceNamespace + templateName);
            if (stream == null)
            {
                throw new FileNotFoundException(
                    "Could not load HTML template embedded resource " + templateName,
                    templateName);
            }
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

    }
}

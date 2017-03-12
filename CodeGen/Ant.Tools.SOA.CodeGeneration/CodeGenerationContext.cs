using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml.Schema;
using System.ServiceModel.Description;
using System.Xml.Serialization;
using System.Xml;

using Microsoft.CSharp;

using Ant.Tools.SOA.CodeGeneration.Options;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Encapsulates the information required by the <see cref="CodeGenerator"/>.
    /// </summary>
    public class CodeGeneratorContext : ICodeGeneratorContext
    {
        /// <summary>
        /// Gets the xml scheam set.
        /// </summary>
        public XmlSchemas XmlSchemas { get; private set; }

        /// <summary>
        /// Gets the code generator options.
        /// </summary>
        public CodeGenOptions CodeGenOptions { get; private set; }

        /// <summary>
        /// Gets the Type name to XmlSchemaType mapping
        /// </summary>
        public IDictionary<string, XmlSchemaType> TypeName2schemaTypeMapping { get; private set; }

        /// <summary>
        /// Gets the XmlQualified element name to type name mapping
        /// </summary>
        public IDictionary<XmlQualifiedName, string> ElementName2TypeNameMapping { get; private set; }

        /// <summary>
        /// Get the XmlQualified element name to TargetNamespace mapping
        /// </summary>
        public IDictionary<string, string> ElementName2TargetNamespaceMapping { get; private set; }

        /// <summary>
        /// Type name to CodeTypeDeclaration mapping
        /// </summary>
        public IDictionary<string, CodeTypeDeclaration> CodeTypeMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGeneratorContext"/> class.
        /// </summary>
        /// <param name="xmlSchemaSet">The xml schema set.</param>
        /// <param name="codeGeneratorOptions">The code generator options.</param>
        public CodeGeneratorContext(XmlSchemas xmlSchemas, CodeGenOptions codeGenOptions)
        {
            XmlSchemas = xmlSchemas;
            CodeGenOptions = codeGenOptions;
            TypeName2schemaTypeMapping = new Dictionary<string, XmlSchemaType>();
            ElementName2TypeNameMapping = new Dictionary<XmlQualifiedName, string>();
            ElementName2TargetNamespaceMapping = new Dictionary<string, string>();
        }
    }
}
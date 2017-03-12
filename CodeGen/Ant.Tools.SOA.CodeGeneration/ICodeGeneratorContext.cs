using System.CodeDom;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

using Ant.Tools.SOA.CodeGeneration.Options;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Encapsulates the information required by the <see cref="CodeGenerator"/>.
    /// </summary>
    public interface ICodeGeneratorContext
    {
        /// <summary>
        /// Gets the metadata set.
        /// </summary>
        XmlSchemas XmlSchemas { get; }

        /// <summary>
        /// Gets the code generator options.
        /// </summary>
        CodeGenOptions CodeGenOptions { get; }


        /// <summary>
        /// Gets the Type name to XmlSchemaType mapping
        /// </summary>
        IDictionary<string, XmlSchemaType> TypeName2schemaTypeMapping { get; }

        /// <summary>
        /// Gets the XmlQualified element name to type name mapping
        /// </summary>
        IDictionary<XmlQualifiedName, string> ElementName2TypeNameMapping { get; }

        /// <summary>
        /// Get the XmlQualified element name to TargetNamespace mapping
        /// </summary>
        IDictionary<string, string> ElementName2TargetNamespaceMapping { get; }

        /// <summary>
        /// Type name to CodeTypeDeclaration mapping
        /// </summary>
        IDictionary<string, CodeTypeDeclaration> CodeTypeMap { get; set; }
    }
}

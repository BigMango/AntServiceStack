using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ServiceModel.Description;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Runtime.Serialization;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.CodeWriter;


namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// This class works as the main interface between the client and the code generation API.
    /// </summary>
    public sealed class CodeGenerator
    {
        #region Private fields

        #endregion

        #region Public methods

        /// <summary>
        /// Executes the code generation workflow.
        /// </summary>        
        public CodeWriterOutput GenerateCode(Options.CodeGenOptions options)
        {
            ICodeGeneratorContext codeGeneratorContext = buildCodeGeneratorContext(options);
            ICodeGenerator codeGenerator = this.buildCodeGenerator(options);

            CodeWriterOptions writeOptions = CodeGenOptionsParser.GetCodeWriterOptions(options);
            if (options.OnlyUseDataContractSerializer)
            {
                CodeCompileUnit targetCodeCompileUnit = codeGenerator.GenerateDataContractCode(codeGeneratorContext);
                return CodeWriter.CodeWriter.Write(targetCodeCompileUnit, writeOptions);
            }
            else if (options.GenerateSeparateFilesEachXsd)
            {
                CodeNamespace[] targetCodeNamespaces = codeGenerator.GenerateCodes(codeGeneratorContext);
                return CodeWriter.CodeWriter.Write(targetCodeNamespaces, writeOptions);
            }
            else
            {
                CodeNamespace targetCodeNamespace = codeGenerator.GenerateCode(codeGeneratorContext);
                return CodeWriter.CodeWriter.Write(targetCodeNamespace, writeOptions);
            }
        }

        #endregion

        #region Private methods

        private ICodeGenerator buildCodeGenerator(Options.CodeGenOptions options)
        {
            ICodeGenerator codeGenerator = null;

            if (options.GenerateDataContractsOnly) // Generate from data contract files
            {
                // Generate data contract code
                codeGenerator = new DataContractGenerator();
            }
            else // Generate from wsdl file
            {
                codeGenerator = new InterfaceContractGenerator();
            }

            return codeGenerator;
        }

        private ICodeGeneratorContext buildCodeGeneratorContext(Options.CodeGenOptions options)
        {
            XmlSchemas xmlSchemas = null;
            MetadataResolverOptions metadataResolverOptions = CodeGenOptionsParser.GetMetadataResolverOptions(options);

            // Generate from data contract files
            if (options.GenerateDataContractsOnly)
            {
                xmlSchemas = MetadataFactory.GetXmlSchemaSetFromDataContractFiles(metadataResolverOptions);
            }
            else // Generate from wsdl file
            {
                xmlSchemas = MetadataFactory.GetXmlSchemaSetFromWsdlFile(metadataResolverOptions);
            }

            return new CodeGeneratorContext(xmlSchemas, options);

        }

        #endregion
    }
}

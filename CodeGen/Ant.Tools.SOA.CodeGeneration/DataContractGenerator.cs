using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Schema;
using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.Extensions;
using System.CodeDom.Compiler;
using System.Xml;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Generates a <see cref="CodeNamespace"/> of data contracts from a xml schema set.
    /// </summary>
    public class DataContractGenerator : ICodeGenerator
    {
        /// <summary>
        /// Generates the <see cref="CodeNamespace"/> based on the provide context.
        /// </summary>
        /// <param name="codeGeneratorContext">The code generator context.</param>
        public CodeNamespace GenerateCode(ICodeGeneratorContext codeGeneratorContext)
        {
            CodeGenOptions codeGenOptions = codeGeneratorContext.CodeGenOptions;
            XmlSchemas xmlSchemas = codeGeneratorContext.XmlSchemas;

            CodeNamespace codeNamespace = new CodeNamespace();

            // Generate DataContracts
            const CodeGenerationOptions generationOptions = CodeGenerationOptions.GenerateProperties;
            var exporter = new XmlCodeExporter(codeNamespace);
            var importer = new XmlSchemaImporter(xmlSchemas, generationOptions, new ImportContext(new CodeIdentifiers(), false));

            // TypeName to XmlSchemaType mapping
            IDictionary<string, XmlSchemaType> typeName2schemaTypeMapping = codeGeneratorContext.TypeName2schemaTypeMapping;
            foreach(XmlSchema schema in xmlSchemas)
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    if (element.IsAbstract) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = element.ElementSchemaType;
                }

                foreach (XmlSchemaType complexType in schema.SchemaTypes.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportSchemaType(complexType.QualifiedName);
                    if (CouldBeAnArray(complexType)) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = complexType;
                }
            }

            if (codeNamespace.Types.Count == 0)
            {
                throw new Exception("No types were generated.");
            }

            // Build type name to code type declaration mapping
            codeGeneratorContext.CodeTypeMap = BuildCodeTypeMap(codeNamespace);
            if (codeGenOptions.ForceElementName)
            {
                DataContractGenerator.BuildElementName2TypeNameMapping(codeGeneratorContext);
            }
            if (codeGenOptions.ForceElementNamespace)
            {
                DataContractGenerator.BuildElementName2TargetNamespaceMapping(codeGeneratorContext);
            }

            // Decorate data contracts code
            ICodeExtension codeExtension = new CodeExtension();
            codeExtension.Process(codeNamespace, codeGeneratorContext);

            CodeExtension.RemoveDefaultTypes(codeNamespace);

            return codeNamespace;
        }

        /// <summary>
        /// Generates the <see cref="CodeNamespace"/> based on the provide context.
        /// </summary>
        /// <param name="codeGeneratorContext">The code generator context.</param>
        public CodeNamespace[] GenerateCodes(ICodeGeneratorContext codeGeneratorContext)
        {
            CodeGenOptions codeGenOptions = codeGeneratorContext.CodeGenOptions;
            XmlSchemas xmlSchemas = codeGeneratorContext.XmlSchemas;

            // Generate DataContracts
            const CodeGenerationOptions generationOptions = CodeGenerationOptions.GenerateProperties;

            List<CodeNamespace> codeNamespaces = new List<CodeNamespace>();

            // TypeName to XmlSchemaType mapping
            IDictionary<string, XmlSchemaType> typeName2schemaTypeMapping = codeGeneratorContext.TypeName2schemaTypeMapping;
            foreach (XmlSchema schema in xmlSchemas)
            {
                CodeNamespace codeNamespace = new CodeNamespace();
                codeNamespace.UserData.Add(Constants.SCHEMA, schema);

                XmlCodeExporter exporter = new XmlCodeExporter(codeNamespace);
                XmlSchemaImporter importer = new XmlSchemaImporter(xmlSchemas, generationOptions, new ImportContext(new CodeIdentifiers(), false));

                foreach(XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    if (element.IsAbstract) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = element.ElementSchemaType;
                }

                foreach (XmlSchemaType schemaType in schema.SchemaTypes.Values)
                {
                    if (String.IsNullOrWhiteSpace(schemaType.SourceUri))
                    {
                        schemaType.SourceUri = schema.SourceUri;
                    }
                    XmlTypeMapping typeMapping = importer.ImportSchemaType(schemaType.QualifiedName);
                    if (CouldBeAnArray(schemaType)) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = schemaType;
                }

                if (codeNamespace.Types.Count > 0) codeNamespaces.Add(codeNamespace);
            }

            if (codeNamespaces.Count == 0)
            {
                throw new Exception("No types were generated.");
            }
            // Build type name to code type declaration mapping
            codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespaces.ToArray());
            if (codeGenOptions.ForceElementName)
            {
                DataContractGenerator.BuildElementName2TypeNameMapping(codeGeneratorContext);
            }
            if (codeGenOptions.ForceElementNamespace)
            {
                DataContractGenerator.BuildElementName2TargetNamespaceMapping(codeGeneratorContext);
            }

            for (int i = 0; i < codeNamespaces.Count; i++)
            {
                CodeNamespace codeNamespace = codeNamespaces[i];
                // Decorate data contracts code
                ICodeExtension codeExtension = new CodeExtension();
                codeExtension.Process(codeNamespace, codeGeneratorContext);

                CodeExtension.RemoveDefaultTypes(codeNamespace);

                List<CodeTypeDeclaration> types = new List<CodeTypeDeclaration>();
                XmlSchema schema = codeNamespace.UserData[Constants.SCHEMA] as XmlSchema;

                foreach (XmlSchemaType schemaType in schema.SchemaTypes.Values)
                {
                    foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
                    {
                        if (codeType.Name == schemaType.Name &&
                                schema.SourceUri == schemaType.SourceUri) 
                            types.Add(codeType);
                    }
                }
                codeNamespace.Types.Clear();
                codeNamespace.Types.AddRange(types.ToArray());

                if (codeNamespace.Types.Count == 0)
                    codeNamespaces.RemoveAt(i--);
            }

            return codeNamespaces.ToArray();
        }

        /// <summary>
        /// Generates the <see cref="CodeCompileUnit"/> based on the provide context.
        /// </summary>
        /// <param name="codeGeneratorContext">The code generator context.</param>
        public CodeCompileUnit GenerateDataContractCode(ICodeGeneratorContext codeGeneratorContext)
        {
            CodeCompileUnit codeCompileUnit = CodeExtension.GenerateDataContractCode(codeGeneratorContext.XmlSchemas);
            CodeExtension.RemoveDefaultTypes(codeCompileUnit);

            codeCompileUnit.ImplementsBaijiSerialization(codeGeneratorContext);

            if (codeGeneratorContext.CodeGenOptions.AddCustomRequestInterface)
            {
                DataContractGenerator.BuildElementName2TypeNameMapping(codeGeneratorContext);
                DataContractGenerator.ImplementCustomInterface(codeGeneratorContext, codeCompileUnit);
            }

            if (codeGeneratorContext.CodeGenOptions.ForceElementNamespace)
            {
                DataContractGenerator.BuildElementName2TargetNamespaceMapping(codeGeneratorContext);
            }

            return codeCompileUnit;
        }

        public static IDictionary<string, CodeTypeDeclaration> BuildCodeTypeMap(CodeNamespace codeNamespace)
        {
            IDictionary<string, CodeTypeDeclaration> codeTypeMap = new Dictionary<string, CodeTypeDeclaration>();
            foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
            {
                if (!codeTypeMap.ContainsKey(codeType.Name))
                    codeTypeMap[codeType.Name] = codeType;
            }
            return codeTypeMap;
        }

        public static IDictionary<string, CodeTypeDeclaration> BuildCodeTypeMap(IEnumerable<CodeNamespace> codeNamespaces)
        {
            IDictionary<string, CodeTypeDeclaration> codeTypeMap = new Dictionary<string, CodeTypeDeclaration>();
            foreach (CodeNamespace codeNamespace in codeNamespaces)
            {
                foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
                {
                    if (!codeTypeMap.ContainsKey(codeType.Name))
                        codeTypeMap[codeType.Name] = codeType;
                }
            }
            return codeTypeMap;
        }

        public static IDictionary<string, CodeTypeDeclaration> BuildCodeTypeMap(CodeCompileUnit codeCompileUnit)
        {
            List<CodeNamespace> codeNamespaces = new List<CodeNamespace>();
            foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
            {
                codeNamespaces.Add(codeNamespace);
            }
            return BuildCodeTypeMap(codeNamespaces.ToArray());
        }

        public static void ImplementCustomInterface(ICodeGeneratorContext codeGeneratorContext, CodeCompileUnit codeCompileUnit)
        {
            foreach (CodeNamespace @namespace in codeCompileUnit.Namespaces)
            {
                foreach (CodeTypeDeclaration type in @namespace.Types)
                {
                    if (type.IsClass || type.IsStruct)
                    {
                        bool hasResponseStatus = false;
                        foreach (CodeTypeMember member in type.Members)
                        {
                            var codeMemberProperty = member as CodeMemberProperty;
                            if (codeMemberProperty != null)
                            {
                                if (codeMemberProperty.Name == "ResponseStatus")
                                    hasResponseStatus = true;
                            }
                        }
                        if (!hasResponseStatus && codeGeneratorContext.ElementName2TypeNameMapping.Values.Contains(type.Name))
                        {
                            type.BaseTypes.Add(codeGeneratorContext.CodeGenOptions.CustomRequestInterface);
                        }
                    }
                }
            }
        }

        public static void BuildElementName2TypeNameMapping(ICodeGeneratorContext codeGeneratorContext)
        {
            XmlSchemas xmlSchemas = codeGeneratorContext.XmlSchemas;
            CodeNamespace codeNamespace = new CodeNamespace();
            
            const CodeGenerationOptions generationOptions = CodeGenerationOptions.GenerateProperties;
            var exporter = new XmlCodeExporter(codeNamespace);
            var importer = new XmlSchemaImporter(xmlSchemas, generationOptions, new ImportContext(new CodeIdentifiers(), false));

            IDictionary<XmlQualifiedName, string> elementName2TypeNameMapping = codeGeneratorContext.ElementName2TypeNameMapping;
            foreach (XmlSchema schema in xmlSchemas)
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    elementName2TypeNameMapping[element.QualifiedName] = typeMapping.XsdTypeName;
                }
            }
        }

        public static void BuildElementName2TargetNamespaceMapping(ICodeGeneratorContext codeGeneratorContext)
        {
            XmlSchemas xmlSchemas = codeGeneratorContext.XmlSchemas;
            CodeNamespace codeNamespace = new CodeNamespace();

            const CodeGenerationOptions generationOptions = CodeGenerationOptions.GenerateProperties;
            var exporter = new XmlCodeExporter(codeNamespace);
            var importer = new XmlSchemaImporter(xmlSchemas, generationOptions, new ImportContext(new CodeIdentifiers(), false));

            IDictionary<string, string> elementName2TargetNamespaceMapping = codeGeneratorContext.ElementName2TargetNamespaceMapping;
            foreach (XmlSchema schema in xmlSchemas)
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    elementName2TargetNamespaceMapping[element.QualifiedName.Name] = schema.TargetNamespace;
                }
            }
        }

        /// <summary>
        /// Checks whether a given XmlSchemaType could be represented as an array. That is the XmlSchemaType
        /// has to be:
        ///     1. Complex type
        ///     2. ...with no base type
        ///     3. ...has no attributes
        ///     4. ...has only one element
        ///     5. ...whose maxOccurs is > 1
        /// </summary>
        /// <returns></returns>
        internal static bool CouldBeAnArray(XmlSchemaType schematype)
        {
            XmlSchemaComplexType complextype = schematype as XmlSchemaComplexType;
            if (complextype != null)
            {
                if (complextype.Attributes.Count == 0)
                {
                    XmlSchemaSequence sequence = complextype.Particle as XmlSchemaSequence;
                    if (sequence != null)
                    {
                        if (sequence.Items.Count == 1)
                        {
                            XmlSchemaElement element = sequence.Items[0] as XmlSchemaElement;
                            if (element != null)
                            {
                                if (element.MaxOccurs > 1 || (element.MaxOccursString != null && element.MaxOccursString.ToLower() == "unbounded"))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
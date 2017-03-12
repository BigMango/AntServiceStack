using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.CodeDom;
using Ant.Tools.SOA.ServiceDescription;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Linq;

using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.Helpers;
using Ant.Tools.SOA.CodeGeneration.CodeWriter;
using Ant.Tools.SOA.CodeGeneration.Extensions;
using Ant.Tools.SOA.CodeGeneration.Exceptions;
using System.Collections.Specialized;

namespace Ant.Tools.SOA.CodeGeneration
{
    public class InterfaceContractGenerator : ICodeGenerator
    {
        static readonly string SYSTEM_NAMESPACE = "System";
        static readonly string SYSTEM_THREADING_NAMESPACE = "System.Threading";
        static readonly string SYSTEM_THREADING_TASKS_NAMESPACE = "System.Threading.Tasks";
        static readonly string C_SERVICE_STACK_SERVICE_CLIENT_NAMESPACE = "AntServiceStack.ServiceClient";
        static readonly string C_SERVICE_STACK_AUTOMATION_TEST_CLIENT_NAMESPACE = "Ant.Automation.Framework.Lib";
        //static readonly string CTRIP_SOA_COMMON_BASE_RESPONSE_TYPE_NAME = "AbstractResponseType";
        static readonly string HEALTH_CHECK_OPERATION_NAME = "CheckHealth";
        static readonly string MOBILE_REQUEST_HEAD_PROPERTY_NAME = "head";
        static readonly string MOBILE_REQUEST_HEAD_TYPE_NAME = "MobileRequestHead";
        static readonly string HAS_RESPONSE_STATUS_INTERFACE_NAME = "IHasResponseStatus";
        static readonly string HAS_MOBILE_REQUEST_HEAD_INTERFACE_NAME = "IHasMobileRequestHead";

        static readonly string HAS_COMMON_REQUEST_INTERFACE_NAME = "IHasCommonRequest";

        static readonly string ASYNC_REQUEST_TASK_NAME_FORMAT = "Task<{0}>";
        static readonly string NULL_DEFAULT_VALUE = " = null";

        static readonly string SERVICE_CLIENT_BASE_NAME = "ServiceClientBase";
        static readonly string SERVICE_CLIENT_FOR_AUTOMATION_BASE_NAME = "ServiceClient";
        static readonly string SERVICE_CLIENT_ORIGINAL_SERVICE_NAME_FIELD_NAME = "OriginalServiceName";
        static readonly string SERVICE_CLIENT_ORIGINAL_SERVICE_NAMESPACE_FIELD_NAME = "OriginalServiceNamespace";
        static readonly string SERVICE_CLIENT_CODE_GENERATOR_VERSION_FIELD_NAME = "CodeGeneratorVersion";
        static readonly string SERVICE_CLIENT_ORIGINAL_SERVICE_TYPE_FIELD_NAME = "OriginalServiceType";
        static readonly string SERVICE_CLIENT_NON_SLB_SERVICE_TYPE_FIELD_NAME = "NonSLB";

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
            IDictionary<XmlQualifiedName, string> elementName2TypeNameMapping = codeGeneratorContext.ElementName2TypeNameMapping;
            IDictionary<string, string> elementName2TargetNamespaceMapping = codeGeneratorContext.ElementName2TargetNamespaceMapping;

            foreach (XmlSchema schema in xmlSchemas)
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    if (element.IsAbstract) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    if (typeMapping.XsdTypeName == "anyType") continue; // ignore no type element
                    if (string.IsNullOrWhiteSpace(typeMapping.XsdTypeName))
                        throw new Exception("Cannot use anonymous type for Request/Response element: " + element.Name + ".");
                    typeName2schemaTypeMapping[typeMapping.XsdTypeName] = element.ElementSchemaType;
                    elementName2TypeNameMapping[element.QualifiedName] = typeMapping.XsdTypeName;

                    elementName2TargetNamespaceMapping[element.QualifiedName.Name] = schema.TargetNamespace;
                }

                foreach (XmlSchemaType complexType in schema.SchemaTypes.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportSchemaType(complexType.QualifiedName);
                    if (DataContractGenerator.CouldBeAnArray(complexType)) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = complexType;
                }
            }

            if (codeNamespace.Types.Count == 0)
            {
                throw new Exception("No types were generated.");
            }

            // Build type name to code type declaration mapping
            codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespace);

            // Decorate data contracts code
            ICodeExtension codeExtension = new CodeExtension();
            codeExtension.Process(codeNamespace, codeGeneratorContext);

            codeNamespace.ImplementsBaijiSerialization(codeGeneratorContext);

            // Generate interface code
            string wsdlFile = codeGeneratorContext.CodeGenOptions.MetadataLocation;
            InterfaceContract interfaceContract = ServiceDescriptionEngine.GetInterfaceContract(wsdlFile);
            CodeTypeDeclaration codeType;
            CodeNamespaceImportCollection imports;
            this.buildInterfaceCode(codeGeneratorContext, interfaceContract, out codeType, out imports);
            codeNamespace.Types.Add(codeType);
            foreach (CodeNamespaceImport @import in imports)
                codeNamespace.Imports.Add(@import);

            // Import SOA common type namespace before removing code types
            codeNamespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
            // Remove SOA common types since they have already been included in CSerivceStack DLL
            CodeExtension.RemoveSOACommonTypes(codeNamespace);

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

            List<CodeNamespace> codeNamespaces = new List<CodeNamespace>();

            // Generate DataContracts
            const CodeGenerationOptions generationOptions = CodeGenerationOptions.GenerateProperties;
            IDictionary<string, XmlSchemaType> typeName2schemaTypeMapping = codeGeneratorContext.TypeName2schemaTypeMapping;
            IDictionary<XmlQualifiedName, string> elementName2TypeNameMapping = codeGeneratorContext.ElementName2TypeNameMapping;
            IDictionary<string, string> elementName2TargetNamespaceMapping = codeGeneratorContext.ElementName2TargetNamespaceMapping;

            foreach(XmlSchema schema in xmlSchemas)
            {
                CodeNamespace codeNamespace = new CodeNamespace();
                codeNamespace.UserData.Add(Constants.SCHEMA, schema);

                // TypeName to XmlSchemaType mapping
                XmlCodeExporter exporter = new XmlCodeExporter(codeNamespace);
                XmlSchemaImporter importer = new XmlSchemaImporter(xmlSchemas, generationOptions, new ImportContext(new CodeIdentifiers(), false));

                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    if (element.IsAbstract) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    if (typeMapping.XsdTypeName == "anyType") continue; // ignore no type element
                    if (string.IsNullOrWhiteSpace(typeMapping.XsdTypeName))
                        throw new Exception("Cannot use anonymous type for Request/Response element: " + element.Name + ".");
                    typeName2schemaTypeMapping[typeMapping.XsdTypeName] = element.ElementSchemaType;
                    elementName2TypeNameMapping[element.QualifiedName] = typeMapping.XsdTypeName;
                    elementName2TargetNamespaceMapping[element.QualifiedName.Name] = schema.TargetNamespace;
                }

                foreach (XmlSchemaType schemaType in schema.SchemaTypes.Values)
                {
                    if (String.IsNullOrWhiteSpace(schemaType.SourceUri))
                    {
                        schemaType.SourceUri = schema.SourceUri;
                    }
                    XmlTypeMapping typeMapping = importer.ImportSchemaType(schemaType.QualifiedName);
                    if (DataContractGenerator.CouldBeAnArray(schemaType)) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = schemaType;
                }

                if (codeNamespace.Types.Count > 0)
                    codeNamespaces.Add(codeNamespace);
            }

            if (codeNamespaces.Count == 0)
            {
                throw new Exception("No types were generated.");
            }

            // Build type name to code type declaration mapping
            codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespaces.ToArray());

            for (int i = 0; i < codeNamespaces.Count; i++)
            {
                CodeNamespace codeNamespace = codeNamespaces[i];
                // Decorate data contracts code
                ICodeExtension codeExtension = new CodeExtension();
                codeExtension.Process(codeNamespace, codeGeneratorContext);

                // Import SOA common type namespace before removing code types
                codeNamespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
                // Remove SOA common types since they have already been included in CSerivceStack DLL
                CodeExtension.RemoveSOACommonTypes(codeNamespace);

                CodeExtension.RemoveDefaultTypes(codeNamespace);

                XmlSchema schema = codeNamespace.UserData[Constants.SCHEMA] as XmlSchema;
                List<CodeTypeDeclaration> types = new List<CodeTypeDeclaration>();
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

            codeNamespaces.ImplementsBaijiSerialization(codeGeneratorContext);

            //Add Interface CodeNamespace
            CodeNamespace interfaceNamespace = new CodeNamespace(codeGenOptions.ClrNamespace);
            // Generate interface code
            string wsdlFile = codeGeneratorContext.CodeGenOptions.MetadataLocation;
            InterfaceContract interfaceContract = ServiceDescriptionEngine.GetInterfaceContract(wsdlFile);
            CodeTypeDeclaration interfaceType;
            CodeNamespaceImportCollection imports;
            this.buildInterfaceCode(codeGeneratorContext, interfaceContract, out interfaceType, out imports);
            interfaceNamespace.Types.Add(interfaceType);
            foreach (CodeNamespaceImport @import in imports)
                interfaceNamespace.Imports.Add(@import);

            // Import SOA common type namespace before removing code types
            interfaceNamespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
            // Remove SOA common types since they have already been included in CSerivceStack DLL
            CodeExtension.RemoveSOACommonTypes(interfaceNamespace);
            CodeExtension.RemoveDefaultTypes(interfaceNamespace);

            string fileName = null;
            if (codeGeneratorContext.CodeGenOptions.CodeGeneratorMode == CodeGeneratorMode.Service)
                fileName = "I" + interfaceContract.ServiceName.Replace("Interface", string.Empty);
            else fileName = interfaceContract.ServiceName.Replace("Interface", string.Empty) + "Client";
            interfaceNamespace.UserData.Add(Constants.FILE_NAME, fileName);

            codeNamespaces.Add(interfaceNamespace);

            return codeNamespaces.ToArray();
        }

        /// <summary>
        /// Generates the <see cref="CodeCompileUnit"/> based on the provide context.
        /// </summary>
        /// <param name="codeGeneratorContext">The code generator context.</param>
        public CodeCompileUnit GenerateDataContractCode(ICodeGeneratorContext codeGeneratorContext)
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
            IDictionary<XmlQualifiedName, string> elementName2TypeNameMapping = codeGeneratorContext.ElementName2TypeNameMapping;
            IDictionary<string, string> elementName2TargetNamespaceMapping = codeGeneratorContext.ElementName2TargetNamespaceMapping;

            foreach (XmlSchema schema in xmlSchemas)
            {
                foreach (XmlSchemaElement element in schema.Elements.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportTypeMapping(element.QualifiedName);
                    if (element.IsAbstract) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    if (typeMapping.XsdTypeName == "anyType") continue; // ignore no type element
                    if (string.IsNullOrWhiteSpace(typeMapping.XsdTypeName))
                        throw new Exception("Cannot use anonymous type for Request/Response element: " + element.Name + ".");
                    typeName2schemaTypeMapping[typeMapping.XsdTypeName] = element.ElementSchemaType;
                    elementName2TypeNameMapping[element.QualifiedName] = typeMapping.XsdTypeName;

                    elementName2TargetNamespaceMapping[element.QualifiedName.Name] = schema.TargetNamespace;
                }

                foreach (XmlSchemaType complexType in schema.SchemaTypes.Values)
                {
                    XmlTypeMapping typeMapping = importer.ImportSchemaType(complexType.QualifiedName);
                    if (DataContractGenerator.CouldBeAnArray(complexType)) continue;

                    exporter.ExportTypeMapping(typeMapping);
                    typeName2schemaTypeMapping[typeMapping.TypeName] = complexType;
                }
            }

            if (codeNamespace.Types.Count == 0)
            {
                throw new Exception("No types were generated.");
            }

            CodeCompileUnit codeCompileUnit = CodeExtension.GenerateDataContractCode(xmlSchemas);
            CodeExtension.RefineCode(codeCompileUnit);
            codeNamespace = CodeExtension.GenerateCode(codeCompileUnit);
            codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespace);

            // Generate interface code
            string wsdlFile = codeGeneratorContext.CodeGenOptions.MetadataLocation;
            InterfaceContract interfaceContract = ServiceDescriptionEngine.GetInterfaceContract(wsdlFile);
            CodeTypeDeclaration codeType;
            CodeNamespaceImportCollection imports;
            this.buildInterfaceCode(codeGeneratorContext, interfaceContract, out codeType, out imports);
            imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
            codeNamespace = new CodeNamespace(codeGenOptions.ClrNamespace);
            codeNamespace.Types.Add(codeType);
            foreach (CodeNamespaceImport @import in imports)
                codeNamespace.Imports.Add(@import);
            codeCompileUnit.Namespaces.Add(codeNamespace);

            codeCompileUnit.ImplementsBaijiSerialization(codeGeneratorContext);

            // Import SOA common type namespace before removing code types
            List<CodeNamespace> toBeRemovedNamespaces = new List<CodeNamespace>();
            foreach (CodeNamespace @namespace in codeCompileUnit.Namespaces)
            {
                // Remove SOA common types since they have already been included in CSerivceStack DLL
                bool hasTypeRemoved = CodeExtension.RemoveSOACommonTypes(@namespace);
                if (hasTypeRemoved)
                    @namespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
                if (@namespace.Types.Count == 0)
                    toBeRemovedNamespaces.Add(@namespace);
            }

            foreach (CodeNamespace @namespace in toBeRemovedNamespaces)
                codeCompileUnit.Namespaces.Remove(@namespace);

            foreach (CodeNamespace @namespace in codeCompileUnit.Namespaces)
            {
                if (@namespace.Name != codeNamespace.Name)
                    codeNamespace.Imports.Add(new CodeNamespaceImport(@namespace.Name));
            }

            CodeExtension.RemoveDefaultTypes(codeCompileUnit);

            if (codeGeneratorContext.CodeGenOptions.AddCustomRequestInterface)
            {
                DataContractGenerator.ImplementCustomInterface(codeGeneratorContext, codeCompileUnit);
            }

            return codeCompileUnit;
        }

        private void buildInterfaceCode(ICodeGeneratorContext codeGeneratorContext, InterfaceContract interfaceContract, out CodeTypeDeclaration codeType, out CodeNamespaceImportCollection imports)
        {
            IDictionary<XmlQualifiedName, string> ElementName2TypeNameMapping = codeGeneratorContext.ElementName2TypeNameMapping;
            IDictionary<string, CodeTypeDeclaration> CodeTypeMap = codeGeneratorContext.CodeTypeMap;
            imports = new CodeNamespaceImportCollection();

            // generate service or client
            CodeGeneratorMode generatorMode = codeGeneratorContext.CodeGenOptions.CodeGeneratorMode;

            string interfaceName = "I" + interfaceContract.ServiceName.Replace("Interface", string.Empty);
            CodeTypeDeclaration interfaceType = new CodeTypeDeclaration(interfaceName);
            interfaceType.UserData.Add(Constants.GENERATED_TYPE, interfaceName);
            interfaceType.IsClass = false;
            interfaceType.TypeAttributes = TypeAttributes.Public;
            interfaceType.IsInterface = true;
            interfaceType.Comments.Clear();
            // Generate service documentation
            string serviceDoc = "Service interface auto-generated by SOA tool, DO NOT CHANGE!\n\n";
            serviceDoc += "注意，实现该接口的服务在AntServiceStack服务容器中是以new instance per request的形式被初始化的，\n";
            serviceDoc += "也就是说，容器会为每个请求创建一个新的服务实例，并在请求结束时释放(release)，而不是单个\n";
            serviceDoc += "服务实例(singleton)服务所有的请求, 所以请务必不要在服务初始化（例如构造函数中）时做较重的初始化\n";
            serviceDoc += "（例如初始化数据库等）动作，否则对性能有很大影响，如果有较重的初始化动作，\n";
            serviceDoc += "请在服务实现中以静态方式（例如静态构造函数中)一次性完成，或者以IoC注入方式初始化，在服务容器\n";
            serviceDoc += "启动时事先将依赖初始化并注册在容器中，让容器在构造服务实例时自动解析和注入依赖(也可在服务实现中手动解析依赖)，\n";
            serviceDoc += "关于静态和依赖注入初始化的样例，请参考AntServiceStack提供的样例程序.\n\n";

            if (!string.IsNullOrEmpty(interfaceContract.ServiceDocumentation))
            {
                serviceDoc += interfaceContract.ServiceDocumentation;
            }
            CodeDomHelper.CreateSummaryComment(interfaceType.Comments, serviceDoc);

            // Import AntServiceStack.ServiceHost namespace, requried by both client and service sides
            imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_SERVICE_HOST_NAMESPACE));

            if (generatorMode == CodeGeneratorMode.Service)
            {
                // Mark as AntServiceStack supported service
                CodeAttributeDeclaration cServiceAttribute = new
                    CodeAttributeDeclaration("AntServiceInterface");

                string serviceName = interfaceContract.ServiceName;
                CodeAttributeArgument serviceNameArgument = new CodeAttributeArgument(new CodePrimitiveExpression(serviceName));
                cServiceAttribute.Arguments.Add(serviceNameArgument);

                string serviceNamespace = interfaceContract.ServiceNamespace;
                CodeAttributeArgument serviceNamespaceArgument = new CodeAttributeArgument(new CodePrimitiveExpression(serviceNamespace));
                cServiceAttribute.Arguments.Add(serviceNamespaceArgument);

                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                string version = ver.Major.ToString() + "." + ver.Minor.ToString() + "." +
                    ver.Build.ToString() + "." + ver.Revision.ToString();
                CodeAttributeArgument codeGeneratorVersionArgument = new CodeAttributeArgument(new CodePrimitiveExpression(version));
                cServiceAttribute.Arguments.Add(codeGeneratorVersionArgument);
                interfaceType.CustomAttributes.Add(cServiceAttribute);
            }

            //CodeTypeDeclaration healthCheckRequestType = null;
            //CodeTypeDeclaration healthCheckResponseType = null;
            bool hasAsync = false;
            foreach (Operation operation in interfaceContract.OperationsCollection)
            {
                var isHealthCheckOperation = false;

                CodeMemberMethod method = new CodeMemberMethod();
                method.Name = operation.Name;
                if (operation.Name.ToLower() == HEALTH_CHECK_OPERATION_NAME.ToLower()) isHealthCheckOperation = true;

                Message inMessage = operation.Input;
                XmlQualifiedName inMessageElementQName = new XmlQualifiedName(inMessage.Element.ElementName, inMessage.Element.ElementNamespace);
                string requestTypeName = null;
                ElementName2TypeNameMapping.TryGetValue(inMessageElementQName, out requestTypeName);
                Enforce.IsNotNull<string>(requestTypeName, "Fail to retrieve request type from wsdl using innput message element QName : " + inMessageElementQName);

                CodeTypeReference requestTypeReference = new CodeTypeReference(requestTypeName);
                CodeParameterDeclarationExpression methodParam =
                                new CodeParameterDeclarationExpression(requestTypeReference, "request");
                methodParam.Type = requestTypeReference;
                method.Parameters.Add(methodParam);

                Message outMessage = operation.Output;
                Enforce.IsNotNull<Message>(outMessage, "Fail to get out message in operation :  " + operation.Name + ", only requst/response style operation is supported");
                XmlQualifiedName outMessageElementQName = new XmlQualifiedName(outMessage.Element.ElementName, outMessage.Element.ElementNamespace);
                string responseTypeName = null;
                ElementName2TypeNameMapping.TryGetValue(outMessageElementQName, out responseTypeName);
                Enforce.IsNotNull<string>(responseTypeName, "Fail to retrieve response type from wsdl using output message element QName : " + outMessageElementQName);
                if (codeGeneratorContext.CodeGenOptions.GenerateAsyncOperations && outMessageElementQName.Name.EndsWith("AsyncResponse"))
                {
                    method.ReturnType = new CodeTypeReference("Task<" + responseTypeName + ">");
                    hasAsync = true;
                }
                else
                    method.ReturnType = new CodeTypeReference(responseTypeName);

                //  SOA Policy enforcement : response type must extend SOA common AbstractResponseType
                CodeTypeDeclaration responseType = null;
                CodeTypeMap.TryGetValue(responseTypeName, out responseType);
                Enforce.IsNotNull<CodeTypeDeclaration>(responseType, "Weird code generator internal error, please ask soa framework team for help.");

                if (isHealthCheckOperation)
                {
                    //healthCheckResponseType = responseType;
                }

                if (!CodeExtension.HasProperty(responseType, Constants.RESPONSE_STATUS_PROPERTY_NAME, Constants.RESPONSE_STATUS_TYPE_NAME))
                {
                    throw new SOAPolicyViolationException(string.Format(" SOA Policy Violation, response type '{0}' does not include requried {1} property of type {2}",
                        responseTypeName, Constants.RESPONSE_STATUS_PROPERTY_NAME, Constants.RESPONSE_STATUS_TYPE_NAME));
                }
                CodeTypeDeclaration responseStatusType = null;
                CodeTypeMap.TryGetValue(Constants.RESPONSE_STATUS_TYPE_NAME, out responseStatusType);
                Enforce.IsNotNull<CodeTypeDeclaration>(responseStatusType, string.Format("Weird code generator internal error, missing requried {0}, please ask soa framework team for help.", Constants.RESPONSE_STATUS_TYPE_NAME));
                if (!CodeExtension.IsSOACommonType(responseStatusType))
                {
                    throw new SOAPolicyViolationException(string.Format(" SOA Policy Violation, {0} reference is not  SOA Common {1}.", Constants.RESPONSE_STATUS_TYPE_NAME, Constants.RESPONSE_STATUS_TYPE_NAME));
                }

                if (!CodeExtension.HasInterface(responseType, HAS_RESPONSE_STATUS_INTERFACE_NAME))
                {
                    // make response type implement IHasResponseStatus interface
                    responseType.BaseTypes.Add(HAS_RESPONSE_STATUS_INTERFACE_NAME);
                }

                // optional common request handling
                CodeTypeDeclaration requestType = null;
                CodeTypeMap.TryGetValue(requestTypeName, out requestType);
                Enforce.IsNotNull<CodeTypeDeclaration>(requestType, "Weird code generator internal error, please ask soa framework team for help.");

                if (isHealthCheckOperation)
                {
                    //healthCheckRequestType = requestType;
                }

                if (CodeExtension.HasProperty(requestType, MOBILE_REQUEST_HEAD_PROPERTY_NAME, MOBILE_REQUEST_HEAD_TYPE_NAME)
                    && !CodeExtension.HasInterface(requestType, HAS_MOBILE_REQUEST_HEAD_INTERFACE_NAME))
                {
                    requestType.BaseTypes.Add(HAS_MOBILE_REQUEST_HEAD_INTERFACE_NAME);
                }

                if (CodeExtension.HasProperty(responseType, Constants.COMMON_REQUEST_PROPERTY_NAME, Constants.COMMON_REQUEST_TYPE_NAME))
                {
                    CodeTypeDeclaration commonRequestType = null;
                    CodeTypeMap.TryGetValue(Constants.COMMON_REQUEST_TYPE_NAME, out commonRequestType);
                    Enforce.IsNotNull<CodeTypeDeclaration>(commonRequestType, string.Format("Weird code generator internal error, missing requried {0}, please ask soa framework team for help.", Constants.COMMON_REQUEST_TYPE_NAME));
                    if (!CodeExtension.IsSOACommonType(commonRequestType))
                    {
                        throw new SOAPolicyViolationException(string.Format(" SOA Policy Violation, {0} reference is not Ant SOA Common {1}.", Constants.COMMON_REQUEST_TYPE_NAME, Constants.COMMON_REQUEST_TYPE_NAME));
                    }
                    if (!CodeExtension.HasInterface(responseType, HAS_COMMON_REQUEST_INTERFACE_NAME))
                    {
                        // make request type implement IHasCommonRequest interface
                        responseType.BaseTypes.Add(HAS_COMMON_REQUEST_INTERFACE_NAME);
                    }
                }

                // Generate operation documentation
                if (!string.IsNullOrEmpty(operation.Documentation))
                {
                    CodeDomHelper.CreateSummaryComment(method.Comments, operation.Documentation);
                }

                interfaceType.Members.Add(method);
            }

            if (hasAsync)
                imports.Add(new CodeNamespaceImport(Constants.SYSTEM_THREADING_TASKS_NAMESPACE));

            // SOA Policy enforcement : healtch check operation is mandatory
            //if (healthCheckRequestType == null || healthCheckResponseType == null)
            //{
            //    throw new SOAPolicyViolationException(string.Format("SOA Policy Violation, missing mandatory check health operation."));
            //}
            //if (!CodeExtension.IsSOACommonType(healthCheckRequestType) || !CodeExtension.IsSOACommonType(healthCheckResponseType))
            //{
            //    throw new SOAPolicyViolationException(string.Format("SOA Policy Violation, wrong SOA common healthcheck types."));
            //}

            if (generatorMode == CodeGeneratorMode.Service)
            {
                codeType = interfaceType;
                return;
            }

            imports.Add(new CodeNamespaceImport(SYSTEM_NAMESPACE));
            imports.Add(new CodeNamespaceImport(SYSTEM_THREADING_NAMESPACE));
            imports.Add(new CodeNamespaceImport(SYSTEM_THREADING_TASKS_NAMESPACE));
            imports.Add(new CodeNamespaceImport(
                generatorMode == CodeGeneratorMode.Client ? C_SERVICE_STACK_SERVICE_CLIENT_NAMESPACE : C_SERVICE_STACK_AUTOMATION_TEST_CLIENT_NAMESPACE));

            string clientName = interfaceContract.ServiceName.Replace("Interface", string.Empty) + "Client";

            CodeTypeDeclaration clientType = new CodeTypeDeclaration(clientName);
            clientType.UserData.Add(Constants.GENERATED_TYPE, clientName);
            var baseType = new CodeTypeReference(
                generatorMode == CodeGeneratorMode.Client ? SERVICE_CLIENT_BASE_NAME : SERVICE_CLIENT_FOR_AUTOMATION_BASE_NAME,
                new CodeTypeReference[] { new CodeTypeReference(clientName) });
            clientType.BaseTypes.Add(baseType);
            codeType = clientType;

            // Generate client documentation
            string clientDoc = "Service client auto-generated by SOA tool, DO NOT CHANGE!\n\n";
            if (!string.IsNullOrEmpty(interfaceContract.ServiceDocumentation))
            {
                clientDoc += interfaceContract.ServiceDocumentation;
            }
            CodeDomHelper.CreateSummaryComment(clientType.Comments, clientDoc);

            // base constructor
            //CodeConstructor baseConstructor = new CodeConstructor();
            //baseConstructor.Attributes = MemberAttributes.Public;
            //baseConstructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(""));
            //clientType.Members.Add(baseConstructor);

            // public constant string service name and namespace
            CodeMemberField codeMemberField = new CodeMemberField(typeof(string), SERVICE_CLIENT_CODE_GENERATOR_VERSION_FIELD_NAME);
            codeMemberField.Attributes = (codeMemberField.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;
            codeMemberField.InitExpression = new CodePrimitiveExpression(typeof(InterfaceContractGenerator).Assembly.GetName().Version.ToString());
            clientType.Members.Add(codeMemberField);
            codeMemberField = new CodeMemberField(typeof(string), SERVICE_CLIENT_ORIGINAL_SERVICE_NAME_FIELD_NAME);
            codeMemberField.Attributes = (codeMemberField.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;
            codeMemberField.InitExpression = new CodePrimitiveExpression(interfaceContract.ServiceName);
            clientType.Members.Add(codeMemberField);
            codeMemberField = new CodeMemberField(typeof(string), SERVICE_CLIENT_ORIGINAL_SERVICE_NAMESPACE_FIELD_NAME);
            codeMemberField.Attributes = (codeMemberField.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;
            codeMemberField.InitExpression = new CodePrimitiveExpression(interfaceContract.ServiceNamespace);
            clientType.Members.Add(codeMemberField);
            codeMemberField = new CodeMemberField(typeof(string), SERVICE_CLIENT_ORIGINAL_SERVICE_TYPE_FIELD_NAME);
            codeMemberField.Attributes = (codeMemberField.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;
            codeMemberField.InitExpression = new CodePrimitiveExpression(SERVICE_CLIENT_NON_SLB_SERVICE_TYPE_FIELD_NAME);
            clientType.Members.Add(codeMemberField);

            // private constructor with baseUri parameter
            CodeConstructor baseConstructorWithParameter = new CodeConstructor();
            baseConstructorWithParameter.Attributes = MemberAttributes.Private;
            baseConstructorWithParameter.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "baseUri"));
            baseConstructorWithParameter.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("baseUri"));
            clientType.Members.Add(baseConstructorWithParameter);

            // private constructor with serviceName & serviceNamespace parameter
            baseConstructorWithParameter = new CodeConstructor();
            baseConstructorWithParameter.Attributes = MemberAttributes.Private;
            baseConstructorWithParameter.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "serviceName"));
            baseConstructorWithParameter.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("serviceName"));
            baseConstructorWithParameter.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "serviceNamespace"));
            baseConstructorWithParameter.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("serviceNamespace"));
            baseConstructorWithParameter.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "subEnv"));
            baseConstructorWithParameter.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("subEnv"));
            clientType.Members.Add(baseConstructorWithParameter);

            // build methods
            foreach (Operation operation in interfaceContract.OperationsCollection)
            {
                string operationName = operation.Name;
                Message inMessage = operation.Input;
                XmlQualifiedName inMessageElementQName = new XmlQualifiedName(inMessage.Element.ElementName, inMessage.Element.ElementNamespace);
                string requestTypeName = null;
                ElementName2TypeNameMapping.TryGetValue(inMessageElementQName, out requestTypeName);
                Message outMessage = operation.Output;
                XmlQualifiedName outMessageElementQName = new XmlQualifiedName(outMessage.Element.ElementName, outMessage.Element.ElementNamespace);
                string responseTypeName = null;
                ElementName2TypeNameMapping.TryGetValue(outMessageElementQName, out responseTypeName);
                CodeTypeReference responseType = new CodeTypeReference(responseTypeName);

                BuildSyncMethod(clientType, operation, requestTypeName, responseTypeName);
                BuildSyncWithCallbackMethod(clientType, operation, requestTypeName, responseTypeName);
                BuildCreateRequestTaskMethod(clientType, operation, requestTypeName, responseTypeName);
                BuildStartIOCPTaskMethod(clientType, operation, requestTypeName, responseTypeName);
            }
        }

        private void BuildSyncMethod(CodeTypeDeclaration clientType, Operation operation, string requestTypeName, string responseTypeName)
        {
            string methodName = operation.Name;

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;

            CodeTypeReference requestType = new CodeTypeReference(requestTypeName);

            CodeParameterDeclarationExpression methodParam =
                            new CodeParameterDeclarationExpression(requestType, operation.Input.Name);
            methodParam.Type = requestType;
            method.Parameters.Add(methodParam);

            CodeTypeReference responseType = new CodeTypeReference(responseTypeName);
            method.ReturnType = responseType;
            method.Attributes = MemberAttributes.Public;
            clientType.Members.Add(method);

            // return base.Invoke<ResponseType>("methodName", request);
            CodeMethodReferenceExpression methodReferenceExpression = new CodeMethodReferenceExpression(
                new CodeBaseReferenceExpression(),
                "Invoke",
                new CodeTypeReference[] {
                        responseType
                    }
            );

            CodeMethodInvokeExpression methodInvokeExpression = new CodeMethodInvokeExpression(
                methodReferenceExpression,
                new CodeExpression[] {
                        new CodePrimitiveExpression(operation.Name),
                        new CodeVariableReferenceExpression(operation.Input.Name)
                    }
            );

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(methodInvokeExpression);
            method.Statements.Add(returnStatement);

            // Generate operation documentation
            if (!string.IsNullOrEmpty(operation.Documentation))
            {
                CodeDomHelper.CreateSummaryComment(method.Comments, operation.Documentation);
            }
        }

        private void BuildSyncWithCallbackMethod(CodeTypeDeclaration clientType, Operation operation, string requestTypeName, string responseTypeName)
        {
            string methodName = operation.Name;

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;

            CodeTypeReference requestType = new CodeTypeReference(requestTypeName);

            CodeParameterDeclarationExpression methodParam =
                            new CodeParameterDeclarationExpression(requestType, operation.Input.Name);
            methodParam.Type = requestType;
            method.Parameters.Add(methodParam);

            string fallbackName = "getFallback";
            CodeTypeReference fallbackType = new CodeTypeReference(string.Format("Func<{0}>", responseTypeName));
            CodeParameterDeclarationExpression fallbackParam =
                            new CodeParameterDeclarationExpression(fallbackType, fallbackName);
            fallbackParam.Type = fallbackType;
            method.Parameters.Add(fallbackParam);

            CodeTypeReference responseType = new CodeTypeReference(responseTypeName);
            method.ReturnType = responseType;
            method.Attributes = MemberAttributes.Public;
            clientType.Members.Add(method);

            // return base.Invoke<ResponseType>("methodName", request, fallback);
            CodeMethodReferenceExpression methodReferenceExpression = new CodeMethodReferenceExpression(
                new CodeBaseReferenceExpression(),
                "Invoke",
                new CodeTypeReference[] {
                        responseType
                    }
            );

            CodeMethodInvokeExpression methodInvokeExpression = new CodeMethodInvokeExpression(
                methodReferenceExpression,
                new CodeExpression[] {
                        new CodePrimitiveExpression(operation.Name),
                        new CodeVariableReferenceExpression(operation.Input.Name),
                        new CodeVariableReferenceExpression(fallbackName)
                    }
            );

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(methodInvokeExpression);
            method.Statements.Add(returnStatement);

            // Generate operation documentation
            if (!string.IsNullOrEmpty(operation.Documentation))
            {
                CodeDomHelper.CreateSummaryComment(method.Comments, operation.Documentation);
            }
        }

        private void BuildCreateRequestTaskMethod(CodeTypeDeclaration clientType, Operation operation, string requestTypeName, string responseTypeName)
        {
            string methodName = "CreateAsyncTaskOf" + operation.Name;

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;

            CodeTypeReference requestType = new CodeTypeReference(requestTypeName);
            CodeParameterDeclarationExpression methodParam =
                            new CodeParameterDeclarationExpression(requestType, operation.Input.Name);
            methodParam.Type = requestType;
            method.Parameters.Add(methodParam);

            CodeTypeReference cancellationTokenType = new CodeTypeReference("CancellationToken?");
            string cancellationTokenParamName = "cancellationToken";
            CodeParameterDeclarationExpression cancellationTokenMethodParam =
                            new CodeParameterDeclarationExpression(cancellationTokenType, cancellationTokenParamName + NULL_DEFAULT_VALUE);
            cancellationTokenMethodParam.Type = cancellationTokenType;
            method.Parameters.Add(cancellationTokenMethodParam);

            CodeTypeReference taskCreationOptionsType = new CodeTypeReference("TaskCreationOptions?");
            string taskCreationOptionsParamName = "taskCreationOptions";
            CodeParameterDeclarationExpression taskCreationOptionsMethodParam =
                            new CodeParameterDeclarationExpression(taskCreationOptionsType, taskCreationOptionsParamName + NULL_DEFAULT_VALUE);
            taskCreationOptionsMethodParam.Type = taskCreationOptionsType;
            method.Parameters.Add(taskCreationOptionsMethodParam);

            CodeTypeReference responseType = new CodeTypeReference(responseTypeName);
            CodeTypeReference taskType = new CodeTypeReference(string.Format(ASYNC_REQUEST_TASK_NAME_FORMAT, responseTypeName));
            method.ReturnType = taskType;
            method.Attributes = MemberAttributes.Public;
            clientType.Members.Add(method);

            // return base.BeginInvoke<ResponseType>("methodName", request, callback, state);
            CodeMethodReferenceExpression methodReferenceExpression = new CodeMethodReferenceExpression(
                new CodeBaseReferenceExpression(),
                "CreateAsyncTask",
                new CodeTypeReference[] { requestType, responseType }
            );

            CodeMethodInvokeExpression methodInvokeExpression = new CodeMethodInvokeExpression(
                methodReferenceExpression,
                new CodeExpression[] {
                        new CodePrimitiveExpression(operation.Name),
                        new CodeVariableReferenceExpression(operation.Input.Name),
                        new CodeVariableReferenceExpression(cancellationTokenParamName),
                        new CodeVariableReferenceExpression(taskCreationOptionsParamName)
                    }
            );

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(methodInvokeExpression);
            method.Statements.Add(returnStatement);

            // Generate operation documentation
            if (!string.IsNullOrEmpty(operation.Documentation))
            {
                CodeDomHelper.CreateSummaryComment(method.Comments, operation.Documentation);
            }
        }

        private void BuildStartIOCPTaskMethod(CodeTypeDeclaration clientType, Operation operation, string requestTypeName, string responseTypeName)
        {
            string methodName = "StartIOCPTaskOf" + operation.Name;

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;

            CodeTypeReference requestType = new CodeTypeReference(requestTypeName);
            CodeParameterDeclarationExpression methodParam =
                            new CodeParameterDeclarationExpression(requestType, operation.Input.Name);
            methodParam.Type = requestType;
            method.Parameters.Add(methodParam);

            CodeTypeReference responseType = new CodeTypeReference(responseTypeName);
            CodeTypeReference taskType = new CodeTypeReference(string.Format(ASYNC_REQUEST_TASK_NAME_FORMAT, responseTypeName));
            method.ReturnType = taskType;
            method.Attributes = MemberAttributes.Public;
            clientType.Members.Add(method);

            // return base.BeginInvoke<ResponseType>("methodName", request, callback, state);
            CodeMethodReferenceExpression methodReferenceExpression = new CodeMethodReferenceExpression(
                new CodeBaseReferenceExpression(),
                "StartIOCPTask",
                new CodeTypeReference[] { responseType }
            );

            CodeMethodInvokeExpression methodInvokeExpression = new CodeMethodInvokeExpression(
                methodReferenceExpression,
                new CodeExpression[] {
                        new CodePrimitiveExpression(operation.Name),
                        new CodeVariableReferenceExpression(operation.Input.Name)
                    }
            );

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(methodInvokeExpression);
            method.Statements.Add(returnStatement);

            // Generate operation documentation
            if (!string.IsNullOrEmpty(operation.Documentation))
            {
                CodeDomHelper.CreateSummaryComment(method.Comments, operation.Documentation);
            }
        }

        /*
        private bool IsExtendingCommonBaseResponseType(CodeTypeDeclaration codeType, IDictionary<string, CodeTypeDeclaration> CodeTypeMap)
        {
            if (codeType.BaseTypes == null || codeType.BaseTypes.Count == 0) return false;
            foreach (CodeTypeReference baseTypeRef in codeType.BaseTypes)
            {
                if (CTRIP_SOA_COMMON_BASE_RESPONSE_TYPE_NAME.Equals(baseTypeRef.BaseType))
                {
                    return true;
                }
                else
                {
                    CodeTypeDeclaration baseCodeType = null;
                    CodeTypeMap.TryGetValue(baseTypeRef.BaseType, out baseCodeType);
                    if (baseCodeType != null && baseCodeType.IsClass)
                    {
                        return IsExtendingCommonBaseResponseType(baseCodeType, CodeTypeMap);
                    }
                }
            }

            return false;
        }
        */
    }
}

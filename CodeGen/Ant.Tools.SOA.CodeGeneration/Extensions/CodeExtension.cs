using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.Helpers;

namespace Ant.Tools.SOA.CodeGeneration.Extensions
{
    /// <summary>
    /// Base class for code generation extension
    /// </summary>
    public class CodeExtension : ICodeExtension
    {
        #region private fields
        /// <summary>
        /// Sorted list for custom collection
        /// </summary>
        //private readonly SortedList<string, string> CollectionTypes = new SortedList<string, string>();

        /// <summary>
        /// Contains all collection fields.
        /// </summary>
        private readonly List<string> LazyLoadingFields = new List<string>();

        /// <summary>
        /// Contains all collection fields.
        /// </summary>
        protected List<string> CollectionTypesFields = new List<string>();

        /// <summary>
        /// List of public properties
        /// </summary>
        protected List<string> PropertiesListFields = new List<string>();

        /// <summary>
        /// List of private fileds
        /// </summary>
        protected List<string> MemberFieldsListFields = new List<string>();

        /// <summary>
        /// Contains all enums.
        /// </summary>
        private List<string> enumListField;

        /// <summary>
        /// Code generation options
        /// </summary>
        private CodeGenOptions codeGenOptions;

        /// <summary>
        /// A set of schemas for code generation
        /// </summary>
        private XmlSchemas xmlSchemas;

        /// <summary>
        /// type name to XmlSchemaType mapping
        /// </summary>
        private IDictionary<string, XmlSchemaType> typeName2SchemaTypeMapping;

        /// <summary>
        /// type name to CodeTypeDeclaration mapping
        /// </summary>
        private IDictionary<string, CodeTypeDeclaration> codeTypeMap;

        private ICodeGeneratorContext codeGeneratorContext;

        private static string customRequestInterface;

        #endregion

        #region private methods

        /// <summary>
        /// Collections the initilializer statement.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static CodeAssignStatement CollectionInitilializerStatement(string name, CodeTypeReference type, params CodeExpression[] parameters)
        {
            CodeAssignStatement statement;
            // in case of Interface type the new statement must contain concrete class
            if (type.BaseType == typeof(IList<>).Name || type.BaseType == typeof(IList<>).FullName)
            {
                var cref = new CodeTypeReference(typeof(List<>));
                cref.TypeArguments.AddRange(type.TypeArguments);
                statement =
               new CodeAssignStatement(
                                       new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                                       new CodeObjectCreateExpression(cref, parameters));
            }
            else
                statement =
                        new CodeAssignStatement(
                                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                                                new CodeObjectCreateExpression(type, parameters));
            return statement;
        }

        private static string GetShortName(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            if (s == customRequestInterface)
                return s;

            if (s.StartsWith("System.") || s.StartsWith("Windows.") || s.StartsWith("Microsoft.") || s.Contains("AntServiceStack."))
                return s;
            return s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        #endregion

        #region public static methods

        public static bool RemoveSOACommonTypes(CodeNamespace codeNamespae)
        {
            List<CodeTypeDeclaration> toBeRemovedTypes = new List<CodeTypeDeclaration>();
            foreach (CodeTypeDeclaration codeType in codeNamespae.Types)
            {
                if (IsSOACommonType(codeType))
                {
                    toBeRemovedTypes.Add(codeType);
                }

                if (IsSOAMobileCommonType(codeType))
                {
                    toBeRemovedTypes.Add(codeType);
                }
            }
            foreach (CodeTypeDeclaration codeType in toBeRemovedTypes)
            {
                codeNamespae.Types.Remove(codeType);
            }

            return toBeRemovedTypes.Count > 0;
        }

        public static bool HasInterface(CodeTypeDeclaration codeType, string interfaceName)
        {
            foreach (CodeTypeReference codeTypeReferece in codeType.BaseTypes)
            {
                if (interfaceName.Equals(codeTypeReferece.BaseType))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasProperty(CodeTypeDeclaration codeType, string propertyName, string propertyTypeName)
        {
            var property = GetProperty(codeType, propertyName);
            if (property == null) return false;
            if (propertyTypeName == property.Type.BaseType
                || Constants.COMMON_TYPE_NAMESPACE_NAME + "." + propertyTypeName == property.Type.BaseType
                || Constants.MOBILE_COMMON_TYPE_NAMESPACE_NAME + "." + propertyTypeName == property.Type.BaseType)
            {
                return true;
            }
            return false;
        }

        public static CodeMemberProperty GetProperty(CodeTypeDeclaration codeType, string propertyName)
        {
            foreach (CodeTypeMember member in codeType.Members)
            {
                var codeMemberProperty = member as CodeMemberProperty;
                if (codeMemberProperty != null && propertyName.Equals(codeMemberProperty.Name))
                {
                    return codeMemberProperty;
                }
            }
            return null;
        }

        public static bool IsSOACommonType(CodeTypeDeclaration codeType)
        {
            return IsTypeOfNamespace(codeType, Constants.CTRIP_SOA_COMMON_TYPE_NAMESPACE);
        }

        public static bool IsSOAMobileCommonType(CodeTypeDeclaration codeType)
        {
            return IsTypeOfNamespace(codeType, Constants.CTRIP_SOA_MOBILE_COMMON_TYPE_NAMESPACE);
        }

        public static bool IsTypeOfNamespace(CodeTypeDeclaration codeType, string typeNamespace)
        {
            if (typeNamespace == null)
                return false;

            foreach (CodeAttributeDeclaration attribute in codeType.CustomAttributes)
            {
                if (attribute.Name == Constants.XML_TYPE_ATTRIBUTE_NAME
                    || attribute.Name == Constants.DATA_CONTRACT_ATTRIBUTE_NAME
                    || attribute.Name == Constants.COLLECTION_DATA_CONTRACT_ATTRIBUTE_NAME)
                {
                    foreach (CodeAttributeArgument argument in attribute.Arguments)
                    {
                        if (argument.Name == "Namespace")
                        {
                            if (typeNamespace.Equals(((CodePrimitiveExpression)argument.Value).Value))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static void RemoveDefaultTypes(CodeCompileUnit codeCompileUnit)
        {
            List<CodeNamespace> toBeRemoved = new List<CodeNamespace>();
            foreach (CodeNamespace @namespace in codeCompileUnit.Namespaces)
            {
                CodeExtension.RemoveDefaultTypes(@namespace);
                if (@namespace.Types.Count == 0)
                    toBeRemoved.Add(@namespace);
            }
            foreach (CodeNamespace @namespace in toBeRemoved)
                codeCompileUnit.Namespaces.Remove(@namespace);
        }

        public static void RemoveDefaultTypes(CodeNamespace codeNamespace)
        {
            List<CodeTypeDeclaration> toBeRemovedTypes = new List<CodeTypeDeclaration>();
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                if (IsSOACommonType(type) && Constants.DEFAULT_TYPE_MAPPINGS.ContainsKey(GetShortName(type.Name)))
                {
                    toBeRemovedTypes.Add(type);
                }
            }
            foreach (CodeTypeDeclaration type in toBeRemovedTypes)
            {
                codeNamespace.Types.Remove(type);
            }

            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                foreach (CodeTypeReference typeRef in type.BaseTypes)
                {
                    string shortTypeName = GetShortName(typeRef.BaseType);
                    if (Constants.DEFAULT_TYPE_MAPPINGS.ContainsKey(shortTypeName))
                    {
                        typeRef.BaseType = Constants.DEFAULT_TYPE_MAPPINGS[shortTypeName];
                    }

                    foreach (CodeTypeReference argumentRef in typeRef.TypeArguments)
                    {
                        shortTypeName = GetShortName(argumentRef.BaseType);
                        if (Constants.DEFAULT_TYPE_MAPPINGS.ContainsKey(shortTypeName))
                        {
                            argumentRef.BaseType = Constants.DEFAULT_TYPE_MAPPINGS[shortTypeName];
                        }
                    }
                }

                foreach (CodeMemberField field in type.Members.OfType<CodeMemberField>())
                {
                    string shortTypeName = GetShortName(field.Type.BaseType);
                    if (Constants.DEFAULT_TYPE_MAPPINGS.ContainsKey(shortTypeName))
                    {
                        field.Type.BaseType = Constants.DEFAULT_TYPE_MAPPINGS[shortTypeName];
                    }
                }

                foreach (CodeMemberProperty property in type.Members.OfType<CodeMemberProperty>())
                {
                    string shortTypeName = GetShortName(property.Type.BaseType);
                    if (Constants.DEFAULT_TYPE_MAPPINGS.ContainsKey(shortTypeName))
                    {
                        property.Type.BaseType = Constants.DEFAULT_TYPE_MAPPINGS[shortTypeName];
                    }
                }
            }
        }

        public static CodeCompileUnit GenerateDataContractCode(XmlSchemas xmlSchemas)
        {
            var dcImporter = new XsdDataContractImporter();
            dcImporter.Options = new ImportOptions()
            {
                GenerateSerializable = true
            };
            //dcImporter.Options.ReferencedCollectionTypes.Add(typeof(List<>));
            XmlSchemaSet schemaSet = MetadataFactory.CreateXmlSchemaSet();
            foreach (XmlSchema schema in xmlSchemas)
            {
                schemaSet.Add(schema);
            }
            dcImporter.Import(schemaSet);
            return dcImporter.CodeCompileUnit;
        }

        public static CodeNamespace GenerateDataContractCode(CodeGenOptions codeGenOptions, XmlSchemas xmlSchemas)
        {
            CodeCompileUnit codeCompileUnit = GenerateDataContractCode(xmlSchemas);
            CodeNamespace codeNamespace = new CodeNamespace(codeGenOptions.ClrNamespace);
            foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
            {
                codeNamespace.Types.AddRange(ns.Types);
            }

            Func<string, string> getShortName = s =>
            {
                if (s.StartsWith("System.") || s.StartsWith("Windows.") || s.StartsWith("Microsoft."))
                    return s;
                return s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last();
            };
            foreach (CodeTypeDeclaration type in codeNamespace.Types)
            {
                List<CodeTypeReference> toBeRemovedBaseTypes = new List<CodeTypeReference>();
                foreach (CodeTypeReference typeRef in type.BaseTypes)
                {
                    if (typeRef.BaseType == "System.Object" || typeRef.BaseType == "System.Runtime.Serialization.IExtensibleDataObject")
                    {
                        toBeRemovedBaseTypes.Add(typeRef);
                        continue;
                    }

                    typeRef.BaseType = getShortName(typeRef.BaseType);
                    foreach (CodeTypeReference argumentRef in typeRef.TypeArguments)
                    {
                        argumentRef.BaseType = getShortName(argumentRef.BaseType);
                    }
                }

                foreach (CodeTypeReference typeRef in toBeRemovedBaseTypes)
                {
                    type.BaseTypes.Remove(typeRef);
                }

                List<CodeTypeMember> toBeRemovedMembers = new List<CodeTypeMember>();
                foreach (CodeMemberField field in type.Members.OfType<CodeMemberField>())
                {
                    if (field.Type.BaseType == "System.Runtime.Serialization.ExtensionDataObject")
                    {
                        toBeRemovedMembers.Add(field);
                        continue;
                    }

                    field.Type.BaseType = getShortName(field.Type.BaseType);
                    foreach (CodeAttributeDeclaration attribute in field.CustomAttributes)
                    {
                        if (attribute.Name == Constants.DATA_MEMBER_ATTRIBUTE_NAME)
                        {
                            CodeAttributeArgument order = null;
                            foreach (CodeAttributeArgument argument in attribute.Arguments)
                            {
                                if (argument.Name == "Order")
                                {
                                    order = argument;
                                    break;
                                }
                            }
                            if (order != null)
                                attribute.Arguments.Remove(order);
                        }
                    }
                }

                foreach (CodeMemberProperty property in type.Members.OfType<CodeMemberProperty>())
                {
                    if (property.Type.BaseType == "System.Runtime.Serialization.ExtensionDataObject")
                    {
                        toBeRemovedMembers.Add(property);
                        continue;
                    }

                    property.Type.BaseType = getShortName(property.Type.BaseType);
                    foreach (CodeAttributeDeclaration attribute in property.CustomAttributes)
                    {
                        if (attribute.Name == Constants.DATA_MEMBER_ATTRIBUTE_NAME)
                        {
                            CodeAttributeArgument order = null;
                            foreach (CodeAttributeArgument argument in attribute.Arguments)
                            {
                                if (argument.Name == "Order")
                                {
                                    order = argument;
                                    break;
                                }
                            }
                            if (order != null)
                                attribute.Arguments.Remove(order);
                        }
                    }
                }

                foreach (CodeTypeMember member in toBeRemovedMembers)
                {
                    type.Members.Remove(member);
                }
            }

            return codeNamespace;
        }

        public static void RefineCode(CodeCompileUnit codeCompileUnit)
        {
            Func<CodeTypeReference, bool> TryUseShortName = t =>
            {
                bool usedShortName = false;
                if (t.BaseType.StartsWith(Constants.MOBILE_COMMON_TYPE_NAMESPACE_NAME)
                    || t.BaseType.StartsWith(Constants.COMMON_TYPE_NAMESPACE_NAME))
                {
                    t.BaseType = GetShortName(t.BaseType);
                    usedShortName = true;
                }
                return usedShortName;
            };

            foreach (CodeNamespace @namespace in codeCompileUnit.Namespaces)
            {
                bool hasCommonTypes = false;
                bool hasServiceHostTypes = false;
                foreach (CodeTypeDeclaration type in @namespace.Types)
                {
                    foreach (CodeTypeReference typeRef in type.BaseTypes)
                    {
                        bool success = TryUseShortName(typeRef);
                        hasCommonTypes |= success;
                        foreach (CodeTypeReference argumentRef in typeRef.TypeArguments)
                        {
                            success = TryUseShortName(typeRef);
                            hasCommonTypes |= success;
                        }
                    }

                    foreach (CodeMemberField field in type.Members.OfType<CodeMemberField>())
                    {
                        bool success = TryUseShortName(field.Type);
                        hasCommonTypes |= success;
                    }

                    foreach (CodeMemberProperty property in type.Members.OfType<CodeMemberProperty>())
                    {
                        bool success = TryUseShortName(property.Type);
                        hasCommonTypes |= success;

                        if (property.Type.BaseType == Constants.RESPONSE_STATUS_TYPE_NAME && property.Name == Constants.RESPONSE_STATUS_PROPERTY_NAME
                            || property.Type.BaseType == Constants.COMMON_REQUEST_TYPE_NAME && property.Name == Constants.COMMON_REQUEST_PROPERTY_NAME)
                            hasServiceHostTypes = true;
                    }
                }

                if (hasCommonTypes
                    && @namespace.Name != Constants.COMMON_TYPE_NAMESPACE_NAME
                    && @namespace.Name != Constants.MOBILE_COMMON_TYPE_NAMESPACE_NAME)
                {
                    @namespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_COMMON_TYPES_NAMESPACE));
                }

                if (hasServiceHostTypes)
                    @namespace.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_SERVICE_HOST_NAMESPACE));
            }
        }

        public static void RefineCodeWithShortName(CodeCompileUnit codeCompileUnit)
        {
            foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
            {
                foreach (CodeTypeDeclaration type in codeNamespace.Types)
                {
                    foreach (CodeTypeReference typeRef in type.BaseTypes)
                    {
                        typeRef.BaseType = GetShortName(typeRef.BaseType);

                        foreach (CodeTypeReference argumentRef in typeRef.TypeArguments)
                        {
                            argumentRef.BaseType = GetShortName(argumentRef.BaseType);
                        }
                    }

                    foreach (CodeMemberField field in type.Members.OfType<CodeMemberField>())
                    {
                        field.Type.BaseType = GetShortName(field.Type.BaseType);
                    }

                    foreach (CodeMemberProperty property in type.Members.OfType<CodeMemberProperty>())
                    {
                        property.Type.BaseType = GetShortName(property.Type.BaseType);
                    }
                }
            }
        }

        public static CodeNamespace GenerateCode(CodeCompileUnit codeCompileUnit)
        {
            CodeNamespace codeNamespace = new CodeNamespace();
            List<string> imports = new List<string>();
            foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
            {
                codeNamespace.Types.AddRange(ns.Types);
                foreach (CodeNamespaceImport @import in ns.Imports)
                {
                    if (!imports.Contains(@import.Namespace))
                        imports.Add(@import.Namespace);
                }
            }

            foreach (string @import in imports)
            {
                if (@import.StartsWith("System.") || @import.StartsWith("Windows.") || 
                    @import.StartsWith("Microsoft.") || @import.StartsWith("AntServiceStack.") || 
                    @import == "System" || @import == "Windows" || @import == "Microsoft" ||
                    @import == "AntServiceStack" || @import.StartsWith("AntServiceStack.Baiji"))
                    codeNamespace.Imports.Add(new CodeNamespaceImport(@import));
            }

            return codeNamespace;
        }

        #endregion

        /// <summary>
        /// Process code for extension
        /// </summary>
        /// <param name="code">CodeNamespace generated</param>
        /// <param name="codeGeneratorContext">Code generator context</param>
        public virtual void Process(CodeNamespace code, ICodeGeneratorContext codeGeneratorContext)
        {
            // make visible to instance level
            codeGenOptions = codeGeneratorContext.CodeGenOptions;
            xmlSchemas = codeGeneratorContext.XmlSchemas;
            typeName2SchemaTypeMapping = codeGeneratorContext.TypeName2schemaTypeMapping;
            this.codeGeneratorContext = codeGeneratorContext;
            customRequestInterface = codeGeneratorContext.CodeGenOptions.CustomRequestInterface;

            this.codeTypeMap = codeGeneratorContext.CodeTypeMap;

            this.ImportNamespaces(code);
            var types = new CodeTypeDeclaration[code.Types.Count];
            code.Types.CopyTo(types, 0);

            enumListField = (from p in types
                             where p.IsEnum
                             select p.Name).ToList();

            foreach (var type in types)
            {
                //CollectionTypes.Clear();
                LazyLoadingFields.Clear();
                CollectionTypesFields.Clear();

                // Remove default remarks attribute
                type.Comments.Clear();

                if (type.IsEnum)
                {
                    this.CreateSummaryCommentForEnum(type);

                    CreateDataContractAttribute(type);
                    CreateEnumMemberAttribute(type);

                    CreateProtoContractAttribute(type);
                    CreateProtoEnumAttribute(type);

                    continue;
                }

                if (type.IsClass || type.IsStruct)
                {
                    this.ProcessClass(code, type);
                }

                CodeConstructor constructor = null;
                foreach (CodeTypeMember member in type.Members)
                {
                    if (member is CodeConstructor)
                    {
                        constructor = member as CodeConstructor;
                        break;
                    }
                }
                if (constructor != null)
                    type.Members.Remove(constructor);
            }
        }

        /// <summary>
        /// Process the class
        /// </summary>
        /// <param name="codeNamespace">The code namespace</param>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        protected virtual void ProcessClass(CodeNamespace codeNamespace, CodeTypeDeclaration type)
        {
            MemberFieldsListFields.Clear();
            PropertiesListFields.Clear();

            if (codeGenOptions.EnableDataBinding)
            {
                type.BaseTypes.Add(typeof(INotifyPropertyChanged));
            }
            
            // Generate WCF style DataContract
            this.CreateDataContractAttribute(type);

            CreateProtoContractAttribute(type);

            if (codeGenOptions.EnableSummaryComment)
            {
                this.CreateSummaryCommentForType(type);
            }

            bool hasResponseStatus = false;
            foreach (CodeTypeMember member in type.Members)
            {
                // Remove default remarks attribute
                member.Comments.Clear();

                var codeMember = member as CodeMemberField;
                if (codeMember != null)
                {
                    MemberFieldsListFields.Add(codeMember.Name);
                    this.ProcessFields(codeMember, codeNamespace);
                }

                var codeMemberProperty = member as CodeMemberProperty;
                if (codeMemberProperty != null)
                {
                    PropertiesListFields.Add(codeMemberProperty.Name);
                    this.ProcessProperty(type, codeNamespace, codeMemberProperty);

                    if (codeMemberProperty.Name == "ResponseStatus")
                        hasResponseStatus = true;
                }
            }

            if (codeGenOptions.EnableDataBinding)
                this.CreateDataBinding(type);

            // Remove property name specified since 
            // it is not supported by first version of  SOA code generation
            RemovePropertyNameSpecified(type);

            int memberOrder = 1;
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeMemberProperty)
                {
                    CreateDataMemberAttribute(type, (CodeMemberProperty)member);
                    CreateProtoMemberAttribute((CodeMemberProperty)member, memberOrder);
                    memberOrder++;
                }
            }

            if (codeGenOptions.AddCustomRequestInterface)
            {
                if (!hasResponseStatus && codeGeneratorContext.ElementName2TypeNameMapping.Values.Contains(type.Name))
                {
                    type.BaseTypes.Add(codeGenOptions.CustomRequestInterface);
                }
            }
        }

        /// <summary>
        /// Remove the property name specified since it is not support  SOA code generation.
        /// </summary>
        /// <param name="type">The type.</param>
        private void RemovePropertyNameSpecified(CodeTypeDeclaration type)
        {
            foreach (var propertyName in PropertiesListFields)
            {
                if (!propertyName.EndsWith("Specified"))
                {
                    CodeMemberProperty specifiedProperty = null;
                    // Search in all properties if PropertyNameSpecified exist
                    string searchPropertyName = string.Format("{0}Specified", propertyName);
                    specifiedProperty = CodeDomHelper.FindProperty(type, searchPropertyName);

                    if (specifiedProperty != null)
                    {
                        type.Members.Remove(specifiedProperty);
                        var field = CodeDomHelper.FindField(type, CodeDomHelper.GetSpecifiedFieldName(propertyName));
                        if (field != null)
                        {
                            type.Members.Remove(field);
                        }
                    }
                    
                }
            }
        }

        /*
        /// <summary>
        /// Creates the collection class.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="collName">Name of the coll.</param>
        protected virtual void CreateCollectionClass(CodeNamespace codeNamespace, string collName)
        {
            var ctd = new CodeTypeDeclaration(collName) { IsClass = true };
            ctd.BaseTypes.Add(new CodeTypeReference(typeof(CollectionBase).FullName, new[] { new CodeTypeReference(CollectionTypes[collName]) }));

            ctd.IsPartial = true;

            bool newCTor = false;
            var ctor = this.GetConstructor(ctd, ref newCTor);

            ctd.Members.Add(ctor);
            codeNamespace.Types.Add(ctd);
        }
        */

        /// <summary>
        /// Create data binding
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateDataBinding(CodeTypeDeclaration type)
        {
            // -------------------------------------------------------------------------------
            // public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            // -------------------------------------------------------------------------------
            var propertyChangedEvent =
                new CodeMemberEvent
                {
                    Attributes = MemberAttributes.Final | MemberAttributes.Public,
                    Name = "PropertyChanged",
                    Type =
                        new CodeTypeReference(typeof(PropertyChangedEventHandler))
                };
            propertyChangedEvent.ImplementationTypes.Add(new CodeTypeReference("INotifyPropertyChanged"));
            type.Members.Add(propertyChangedEvent);

            var propertyChangedMethod = CodeDomHelper.CreatePropertyChangedMethod();

            type.Members.Add(propertyChangedMethod);
        }

        private bool IsPropertyOfByteArrayType(CodeMemberProperty prop)
        {
            if (prop.Type.ArrayElementType != null && prop.Type.BaseType == typeof(byte).FullName)
                return true;
            return false;
        }

        /// <summary>
        /// Property process
        /// </summary>
        /// <param name="type">Represents a type declaration for a class, structure, interface, or enumeration</param>
        /// <param name="ns">The CodeNamespace</param>
        /// <param name="member">Type members include fields, methods, properties, constructors and nested types</param>
        protected virtual void ProcessProperty(CodeTypeDeclaration type, CodeNamespace ns, CodeTypeMember member)
        {
            if (codeGenOptions.EnableSummaryComment)
            {
                XmlSchemaType xmlSchemaType = null;
                this.typeName2SchemaTypeMapping.TryGetValue(type.Name, out xmlSchemaType);
                var xmlComplexType = xmlSchemaType as XmlSchemaComplexType;
                bool foundInAttributes = false;
                if (xmlComplexType != null)
                {
                    // Search property in attributes for summary comment generation
                    foreach (XmlSchemaObject attribute in xmlComplexType.Attributes)
                    {
                        var xmlAttrib = attribute as XmlSchemaAttribute;
                        if (xmlAttrib != null)
                        {
                            if (member.Name.Equals(xmlAttrib.QualifiedName.Name))
                            {
                                this.CreateCommentFromAnnotation(xmlAttrib.Annotation, member.Comments);
                                foundInAttributes = true;
                            }
                        }
                    }

                    // Search property in XmlSchemaElement for summary comment generation
                    if (!foundInAttributes)
                    {
                        XmlSchemaGroupBase elementGroup = xmlComplexType.ContentTypeParticle as XmlSchemaGroupBase;
                        if (elementGroup == null)
                            elementGroup = xmlComplexType.Particle as XmlSchemaGroupBase;
                        if (elementGroup != null)
                        {
                            foreach (XmlSchemaObject item in elementGroup.Items)
                            {
                                var currentItem = item as XmlSchemaElement;
                                if (currentItem != null)
                                {
                                    if (member.Name.Equals(currentItem.QualifiedName.Name))
                                        this.CreateCommentFromAnnotation(currentItem.Annotation, member.Comments);
                                }
                            }
                        }
                    }
                }
            }

            var prop = (CodeMemberProperty)member;

            if (prop.Type.ArrayElementType != null)
            {
                prop.Type = this.GetCollectionType(prop.Type);
                CollectionTypesFields.Add(prop.Name);
            }

            if (codeGenOptions.EnableInitializeFields)
            {
                var propReturnStatment = prop.GetStatements[0] as CodeMethodReturnStatement;
                if (propReturnStatment != null)
                {
                    var field = propReturnStatment.Expression as CodeFieldReferenceExpression;
                    if (field != null)
                    {
                        if (LazyLoadingFields.IndexOf(field.FieldName) != -1) 
                        {
                            if (IsPropertyOfByteArrayType(prop)) // ignore byte array lazy loading
                            {
                                CodeDomHelper.CreateNonDococumentComment(prop.Comments, "CODEGEN WARNING : lazy loading ignored for property of byte array type");
                            }
                            else
                            {
                                prop.GetStatements.Insert(0, this.CreateInstanceIfNotNull(field.FieldName, prop.Type));
                            }
                        }
                    }
                }
            }

            // Add OnPropertyChanges in setter
            if (codeGenOptions.EnableDataBinding)
            {
                if (type.BaseTypes.IndexOf(new CodeTypeReference(typeof(CollectionBase))) == -1)
                {
                    // -----------------------------
                    // if (handler != null) {
                    //    OnPropertyChanged("PropertyName");
                    // -----------------------------
                    CodeExpression[] propertyChangeParams = new CodeExpression[] { new CodePrimitiveExpression(prop.Name) };

                    var propChange = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "OnPropertyChanged"), propertyChangeParams);

                    var propAssignStatment = prop.SetStatements[0] as CodeAssignStatement;
                    if (propAssignStatment != null)
                    {
                        var cfreL = propAssignStatment.Left as CodeFieldReferenceExpression;
                        var cfreR = propAssignStatment.Right as CodePropertySetValueReferenceExpression;

                        if (cfreL != null)
                        {
                            var setValueCondition = new CodeStatementCollection { propAssignStatment, propChange };

                            // ---------------------------------------------
                            // if ((xxxField.Equals(value) != true)) { ... }
                            // ---------------------------------------------
                            var condStatmentCondEquals = new CodeConditionStatement(
                                new CodeBinaryOperatorExpression(
                                    new CodeMethodInvokeExpression(
                                        new CodeFieldReferenceExpression(
                                            null,
                                            cfreL.FieldName),
                                        "Equals",
                                        cfreR),
                                    CodeBinaryOperatorType.IdentityInequality,
                                    new CodePrimitiveExpression(true)),
                                CodeDomHelper.CodeStmtColToArray(setValueCondition));

                            // ---------------------------------------------
                            // if ((xxxField != null)) { ... }
                            // ---------------------------------------------
                            var condStatmentCondNotNull = new CodeConditionStatement(
                                new CodeBinaryOperatorExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeThisReferenceExpression(), cfreL.FieldName),
                                        CodeBinaryOperatorType.IdentityInequality,
                                        new CodePrimitiveExpression(null)),
                                        new CodeStatement[] { condStatmentCondEquals },
                                        CodeDomHelper.CodeStmtColToArray(setValueCondition));

                            var property = member as CodeMemberProperty;
                            if (property != null)
                            {
                                if (property.Type.BaseType != new CodeTypeReference(typeof(long)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(DateTime)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(float)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(double)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(int)).BaseType &&
                                    property.Type.BaseType != new CodeTypeReference(typeof(bool)).BaseType &&
                                    enumListField.IndexOf(property.Type.BaseType) == -1)
                                    prop.SetStatements[0] = condStatmentCondNotNull;
                                else
                                    prop.SetStatements[0] = condStatmentCondEquals;
                            }
                        }
                        else
                            prop.SetStatements.Add(propChange);
                    }
                }
            }
        }

        /// <summary>
        /// Field process.
        /// </summary>
        /// <param name="member">CodeTypeMember member</param>
        /// <param name="ctor">CodeMemberMethod constructor</param>
        /// <param name="ns">CodeNamespace</param>
        /// <param name="addedToConstructor">Indicates if create a new constructor</param>
        protected virtual void ProcessFields(CodeTypeMember member,
                                             CodeNamespace ns)
        {
            var field = (CodeMemberField)member;

            // ------------------------------------------
            // protected virtual  List <Actor> nameField;
            // ------------------------------------------
            var thisIsCollectionType = field.Type.ArrayElementType != null;
            if (thisIsCollectionType)
            {
                field.Type = this.GetCollectionType(field.Type);
            }

            if (codeGenOptions.EnableInitializeFields)
            {
                if (codeGenOptions.GenerateTypedLists || codeGenOptions.GenerateCollections)
                {
                    CodeTypeDeclaration declaration = null;
                    this.codeTypeMap.TryGetValue(field.Type.BaseType, out declaration);
                    if (thisIsCollectionType)
                    {
                        if (codeGenOptions.EnableLazyLoading)
                        {
                            LazyLoadingFields.Add(field.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create new instance of object
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="type">CodeTypeReference Type</param>
        /// <returns>return instance CodeConditionStatement</returns>
        protected virtual CodeConditionStatement CreateInstanceIfNotNull(string name, CodeTypeReference type, params CodeExpression[] parameters)
        {

            CodeAssignStatement statement;
            if (type.BaseType.Equals("System.String") && type.ArrayRank == 0)
            {
                statement =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeDomHelper.GetStaticField(typeof(String), "Empty"));
            }
            else
            {
                statement = CollectionInitilializerStatement(name, type, parameters);
            }

            return
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                        new CodeStatement[] { statement });
        }

        /// <summary>
        /// Create new instance of object
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <param name="type">CodeTypeReference Type</param>
        /// <returns>return instance CodeConditionStatement</returns>
        protected virtual CodeAssignStatement CreateInstance(string name, CodeTypeReference type)
        {
            CodeAssignStatement statement;
            if (type.BaseType.Equals("System.String") && type.ArrayRank == 0)
            {
                statement = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name), CodeDomHelper.GetStaticField(typeof(String), "Empty"));
            }
            else
            {
                statement = CollectionInitilializerStatement(name, type);
            }
            return statement;
        }


        /// <summary>
        /// Get CodeTypeReference for collection
        /// </summary>
        /// <param name="codeType">The code Type.</param>
        /// <returns>return array of or generic collection</returns>
        protected virtual CodeTypeReference GetCollectionType(CodeTypeReference codeType)
        {
            CodeTypeReference collectionType = codeType;
            if (codeType.BaseType == typeof(byte).FullName)
            {
                // Never change byte[] to List<byte> etc.
                // Fix : when translating hexBinary and base64Binary
                return codeType;
            }

            if (codeGenOptions.GenerateTypedLists)
            {
                if (codeGenOptions.EnableBaijiSerialization && codeType.BaseType.StartsWith("System.Nullable"))
                    collectionType = new CodeTypeReference("List", new[] { new CodeTypeReference("System.Nullable", new[] { new CodeTypeReference(codeType.TypeArguments[0].BaseType) }) });
                else
                    collectionType = new CodeTypeReference("List", new[] { new CodeTypeReference(codeType.BaseType) });
            }
            else if (codeGenOptions.GenerateCollections)
            {
                if (codeGenOptions.EnableBaijiSerialization && codeType.BaseType.StartsWith("System.Nullable"))
                    collectionType = new CodeTypeReference("ObservableCollection", new[] { new CodeTypeReference("System.Nullable", new[] { new CodeTypeReference(codeType.TypeArguments[0].BaseType) }) });
                else
                    collectionType = new CodeTypeReference("ObservableCollection", new[] { new CodeTypeReference(codeType.BaseType) });
            } 
            else 
            {
                // If not use generic, remove multiple array Ex. string[][] => string[]
                if (codeType.ArrayElementType.ArrayRank > 0)
                    collectionType.ArrayElementType.ArrayRank = 0;
            }

            return collectionType;
        }


        protected virtual CodeConstructor GetConstructor(CodeTypeDeclaration type, ref bool newCTor)
        {
            CodeConstructor ctor = null;
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeConstructor)
                {
                    ctor = member as CodeConstructor;
                }
            }

            if (ctor == null)
            {
                newCTor = true;
                ctor = this.CreateClassConstructor(type);
            }

            if (codeGenOptions.EnableSummaryComment)
            {
                CodeDomHelper.CreateSummaryComment(ctor.Comments, string.Format("{0} class constuctor", ctor.Name));
            }

            return ctor;
        }

        /// <summary>
        /// Create a Class Constructor
        /// </summary>
        /// <param name="type">type of declaration</param>
        /// <returns>return CodeConstructor</returns>
        protected virtual CodeConstructor CreateClassConstructor(CodeTypeDeclaration type)
        {
            var ctor = new CodeConstructor { Attributes = MemberAttributes.Public, Name = type.Name };
            return ctor;
        }

        /// <summary>
        /// Create data contract attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateDataContractAttribute(CodeTypeDeclaration type)
        {
            string @namespace = string.Empty;
            string messageName = string.Empty;

            var elements = (from item in codeGeneratorContext.ElementName2TypeNameMapping
                            where item.Value == type.Name
                            select item.Key.Name).Distinct().ToList();
            if (elements.Count == 1) // 单个映射，一个 type 对应 一个 element 时，消息名称 就是 schema 定义的 element name
            {
                messageName = elements[0];
            }
            else if (elements.Count > 1) // 多个映射，一个 type 对应 多个 element 时，消息名称 设置为类型的名字
            {
                // 以 RequestType/ResponseType 结尾时，去掉 Type
                if (type.Name.EndsWith("RequestType") || type.Name.EndsWith("ResponseType"))
                    messageName = type.Name.Substring(0, type.Name.LastIndexOf('T'));
                else
                    messageName = type.Name;

                // 多个映射时，消息的 Namespace 以类型的 Namespace 为准
                var typeNamespace = codeGeneratorContext.TypeName2schemaTypeMapping[type.Name].QualifiedName.Namespace;
                codeGeneratorContext.ElementName2TargetNamespaceMapping[messageName] = typeNamespace;
            }
            // else  这个类型不作为消息

            if (codeGeneratorContext.CodeGenOptions.GenerateAsyncOperations && !string.IsNullOrWhiteSpace(messageName)) // 识别异步消息名称，并进行转化
            {
                if (messageName.EndsWith("AsyncRequest") || messageName.EndsWith("AsyncResponse"))
                {
                    var asyncMessageNamespace = codeGeneratorContext.ElementName2TargetNamespaceMapping[messageName];
                    messageName = messageName.Remove(messageName.LastIndexOf('A'), 5);
                    codeGeneratorContext.ElementName2TargetNamespaceMapping[messageName] = asyncMessageNamespace;
                }
            }

            // 获取 XmlRootAttribute 和 XmlTypeAttribute
            CodeAttributeDeclaration xmlRootDecl = null, xmlTypeDecl = null;
            foreach (CodeAttributeDeclaration attribute in type.CustomAttributes)
            {
                if (attribute.Name == Constants.XML_ROOT_ATTRIBUTE_NAME)
                    xmlRootDecl = attribute;
                else if (attribute.Name == Constants.XML_TYPE_ATTRIBUTE_NAME)
                    xmlTypeDecl = attribute;
            }

            // 获取用于 DataContractAttribute 的 Namespace
            if (codeGeneratorContext.CodeGenOptions.ForceElementNamespace)
            {
                // 若规范化消息命名空间，并且是一个消息，则从 Schema 的 targetNamespace 获取
                if (!string.IsNullOrWhiteSpace(messageName))
                    @namespace = codeGeneratorContext.ElementName2TargetNamespaceMapping[messageName];
            }
            if (string.IsNullOrWhiteSpace(@namespace) && xmlTypeDecl != null)  //若 Namespace为空，使用 XmlTypeAttribute 中的 Namespace （这是老的逻辑，为了兼容已经上线的服务）
            {
                var xmlTypeArguments = xmlTypeDecl.Arguments.OfType<CodeAttributeArgument>().ToList();
                var xmlTypeNamespaceArgument = xmlTypeArguments.SingleOrDefault(item => item.Name == "Namespace");

                if (xmlTypeNamespaceArgument != null)
                    @namespace = Convert.ToString(((CodePrimitiveExpression)xmlTypeNamespaceArgument.Value).Value);
            }

            // 1. 获取用于 DataContractAttribute 的 ElementName， 
            // 2. 规范化时，设置 XmlRootAttribute 的 ElementName 和 Namespace
            if (xmlRootDecl != null)
            {
                var xmlRootArguments = xmlRootDecl.Arguments.OfType<CodeAttributeArgument>().ToList();
                var xmlRootElementNameArgument = xmlRootArguments.SingleOrDefault(item => item.Name == String.Empty || item.Name == "ElementName");
                if (xmlRootElementNameArgument == null) // 若没有 ElementName 参数，则添加之
                {
                    if (codeGeneratorContext.CodeGenOptions.ForceElementName && !string.IsNullOrWhiteSpace(messageName))
                    {
                        // 规范化消息名称时，设置 element name 为 message name
                        xmlRootDecl.Arguments.Insert(0, new CodeAttributeArgument("", new CodePrimitiveExpression(messageName)));
                    }
                    else
                        messageName = "";
                }
                else // 若有 ElementName 参数
                {
                    if (codeGeneratorContext.CodeGenOptions.ForceElementName) // 规范化时，设置 element name 为 message name
                    {
                        if (!string.IsNullOrWhiteSpace(messageName))
                            xmlRootElementNameArgument.Value = new CodePrimitiveExpression(messageName);
                    }
                    else // 否则，只更新 message name 的值，用于 DataContractAttribute（这是老的逻辑，为了兼容已经上线的服务）
                    {
                        messageName = Convert.ToString(((CodePrimitiveExpression)xmlRootElementNameArgument.Value).Value);
                        if (codeGeneratorContext.CodeGenOptions.GenerateAsyncOperations && !string.IsNullOrWhiteSpace(messageName)) // 识别异步消息名称，并进行转化
                        {
                            if (messageName.EndsWith("AsyncRequest") || messageName.EndsWith("AsyncResponse"))
                            {
                                messageName = messageName.Remove(messageName.LastIndexOf('A'), 5);
                                xmlRootElementNameArgument.Value = new CodePrimitiveExpression(messageName); 
                            }
                        }
                    }
                }

                // 规范化消息命名空间时，设置 XmlRootAttribute 的 Namespace
                var xmlRootNamespaceArgument = xmlRootArguments.SingleOrDefault(item => item.Name == "Namespace");
                if (codeGeneratorContext.CodeGenOptions.ForceElementNamespace && !string.IsNullOrWhiteSpace(@namespace))
                {
                    xmlRootNamespaceArgument.Value = new CodePrimitiveExpression(@namespace);
                }
            }

            // 添加 DataContract 特性，使用前面获取到的 Name 和 Namespace
            CodeAttributeDeclaration attributeDeclaration = new CodeAttributeDeclaration("DataContract");
            string alias = GetTypeAlias(type);
            if (alias != null)
                attributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(alias)));
            else if (!string.IsNullOrWhiteSpace(messageName))
                attributeDeclaration.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(messageName)));
            attributeDeclaration.Arguments.Add(new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(@namespace)));
            type.CustomAttributes.Add(attributeDeclaration);
        }

        /// <summary>
        /// Creates the data member attribute.
        /// </summary>
        /// <param name="property">Represents a declaration for a property of a type.</param>
        protected virtual void CreateDataMemberAttribute(CodeTypeDeclaration type, CodeMemberProperty property)
        {
            CodeAttributeDeclaration attribute = new CodeAttributeDeclaration("DataMember");
            string alias = GetMemberAlias(type, property);
            if (alias != null)
                attribute.Arguments.Add(new CodeAttributeArgument("Name", new CodePrimitiveExpression(alias)));
            property.CustomAttributes.Add(attribute);
        }

        /// <summary>
        /// Create data contract enum attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateEnumMemberAttribute(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeMemberField)
                {
                    CodeMemberField field = (CodeMemberField)member;
                    CodeAttributeDeclaration attribute = new CodeAttributeDeclaration("EnumMember");
                    string alias = GetMemberAlias(type, member);
                    if (alias != null)
                        attribute.Arguments.Add(new CodeAttributeArgument("Value", new CodePrimitiveExpression(alias)));
                    field.CustomAttributes.Add(attribute);
                }
            }
        }

        /// <summary>
        /// Create proto contract attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateProtoContractAttribute(CodeTypeDeclaration type)
        {
            type.CustomAttributes.Add(new CodeAttributeDeclaration("ProtoContract"));
        }

        /// <summary>
        /// Create proto enum attribute
        /// </summary>
        /// <param name="type">Code type declaration</param>
        protected virtual void CreateProtoEnumAttribute(CodeTypeDeclaration type)
        {
            foreach (CodeTypeMember member in type.Members)
            {
                if (member is CodeMemberField)
                {
                    CodeMemberField field = (CodeMemberField)member;
                    field.CustomAttributes.Add(new CodeAttributeDeclaration("ProtoEnum"));
                }
            }
        }

        /// <summary>
        /// Creates the proto member attribute.
        /// </summary>
        /// <param name="property">Represents a declaration for a property of a type.</param>
        protected virtual void CreateProtoMemberAttribute(CodeMemberProperty property, int order)
        {
            property.CustomAttributes.Add(new CodeAttributeDeclaration("ProtoMember", new CodeAttributeArgument(new CodePrimitiveExpression(order))));
        }

        /// <summary>
        /// Create the summary comment for type from schema
        /// </summary>
        /// <param name="codeTypeDeclaration">The code type declaration</param>
        protected virtual void CreateSummaryCommentForType(CodeTypeDeclaration codeTypeDeclaration)
        {
            XmlSchemaType xmlSchemaType = null;
            this.typeName2SchemaTypeMapping.TryGetValue(codeTypeDeclaration.Name, out xmlSchemaType);
            if (xmlSchemaType != null && xmlSchemaType.Annotation != null)
            {
                foreach (var item in xmlSchemaType.Annotation.Items)
                {
                    var xmlDoc = item as XmlSchemaDocumentation;
                    if (xmlDoc == null) continue;
                    this.CreateCommentStatement(codeTypeDeclaration.Comments, xmlDoc);
                }
            }
        }

        /// <summary>
        /// Create the summary comment for enum type from schema
        /// </summary>
        /// <param name="enumTypeDeclaration">The enum type declaration</param>
        protected virtual void CreateSummaryCommentForEnum(CodeTypeDeclaration enumTypeDeclaration)
        {
            XmlSchemaType xmlSchemaType = null;
            this.typeName2SchemaTypeMapping.TryGetValue(enumTypeDeclaration.Name, out xmlSchemaType);
            if (xmlSchemaType != null && xmlSchemaType.Annotation != null)
            {
                // comment on enum type
                foreach (var item in xmlSchemaType.Annotation.Items)
                {
                    var xmlDoc = item as XmlSchemaDocumentation;
                    if (xmlDoc == null) continue;
                    this.CreateCommentStatement(enumTypeDeclaration.Comments, xmlDoc);
                }

                // comment on enum members
                XmlSchemaSimpleType xmlSchemaSimpleType = xmlSchemaType as XmlSchemaSimpleType;
                if (xmlSchemaSimpleType != null)
                {
                    XmlSchemaSimpleTypeRestriction typeRestriction = xmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                    if (typeRestriction != null)
                    {
                        foreach (XmlSchemaObject xmlSchemaObject in typeRestriction.Facets)
                        {
                            XmlSchemaFacet facet = xmlSchemaObject as XmlSchemaFacet;
                            if (facet != null && facet.Annotation != null)
                            {
                                foreach (CodeTypeMember member in enumTypeDeclaration.Members)
                                {
                                    if (member.Name.Equals(facet.Value))
                                    {
                                        this.CreateCommentFromAnnotation(facet.Annotation, member.Comments);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate summary comment from XmlSchemaAnnotation
        /// </summary>
        /// <param name="xmlSchemaAnnotation">XmlSchemaAnnotation from XmlSchemaType</param>
        /// <param name="codeCommentStatementCollection">codeCommentStatementCollection from member</param>
        protected virtual void CreateCommentFromAnnotation(XmlSchemaAnnotation xmlSchemaAnnotation, CodeCommentStatementCollection codeCommentStatementCollection)
        {
            if (xmlSchemaAnnotation != null)
            {
                foreach (XmlSchemaObject annotation in xmlSchemaAnnotation.Items)
                {
                    var xmlDoc = annotation as XmlSchemaDocumentation;
                    if (xmlDoc != null)
                    {
                        this.CreateCommentStatement(codeCommentStatementCollection, xmlDoc);
                    }
                }
            }
        }

        /// <summary>
        /// Create CodeCommentStatement from schema documentation
        /// </summary>
        /// <param name="codeStatementColl">CodeCommentStatementCollection collection</param>
        /// <param name="xmlDoc">Schema documentation</param>
        protected virtual void CreateCommentStatement(
            CodeCommentStatementCollection codeStatementColl,
            XmlSchemaDocumentation xmlDoc)
        {
            if (xmlDoc.Markup == null) return;

            codeStatementColl.Clear();
            foreach (XmlNode itemDoc in xmlDoc.Markup)
            {
                var textLine = itemDoc.InnerText.Trim();
                if (textLine.Length > 0)
                {
                    CodeDomHelper.CreateSummaryComment(codeStatementColl, textLine);
                }
            }
        }

        protected virtual string GetAliasFromAppInfo(XmlSchemaAnnotation annotation)
        {
            if (annotation == null)
                return null;

            XmlSchemaAppInfo appInfo = annotation.Items.OfType<XmlSchemaAppInfo>().FirstOrDefault();
            if (appInfo == null)
                return null;

            XmlNode aliasNode = appInfo.Markup.FirstOrDefault(n => n.Name == "alias");
            if (aliasNode == null)
                return null;

            return string.IsNullOrWhiteSpace(aliasNode.InnerText) ? null : aliasNode.InnerText.Trim();
        }

        protected virtual string GetTypeAlias(CodeTypeDeclaration type)
        {
            if (!typeName2SchemaTypeMapping.ContainsKey(type.Name))
                return null;

            return GetAliasFromAppInfo(typeName2SchemaTypeMapping[type.Name].Annotation);
        }

        protected virtual string GetMemberAlias(CodeTypeDeclaration type, CodeTypeMember member)
        {
            if (!typeName2SchemaTypeMapping.ContainsKey(type.Name))
                return null;

            XmlSchemaType schemaType = typeName2SchemaTypeMapping[type.Name];
            if (schemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexSchemaType = schemaType as XmlSchemaComplexType;
                XmlSchemaGroupBase elementGroup = complexSchemaType.ContentTypeParticle as XmlSchemaGroupBase;
                if (elementGroup == null)
                    elementGroup = complexSchemaType.Particle as XmlSchemaGroupBase;
                if (elementGroup == null)
                    return null;
                XmlSchemaElement element = elementGroup.Items.OfType<XmlSchemaElement>().FirstOrDefault(e => e.Name == member.Name);
                if (element == null)
                    return null;
                return GetAliasFromAppInfo(element.Annotation);
            }
            else if (schemaType is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simpleSchemaType = schemaType as XmlSchemaSimpleType;
                XmlSchemaSimpleTypeRestriction content = simpleSchemaType.Content as XmlSchemaSimpleTypeRestriction;
                if (content == null)
                    return null;
                XmlSchemaFacet facet = content.Facets.OfType<XmlSchemaFacet>().FirstOrDefault(f => f.Value == member.Name);
                if (facet == null)
                    return null;
                return GetAliasFromAppInfo(facet.Annotation);
            }

            return null;
        }

        /// <summary>
        /// Import namespaces
        /// </summary>
        /// <param name="code"></param>
        protected virtual void ImportNamespaces(CodeNamespace code)
        {
            code.Imports.Add(new CodeNamespaceImport("System"));
            code.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            code.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
            code.Imports.Add(new CodeNamespaceImport("System.Collections"));
            code.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
            code.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            code.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization"));

            if (codeGenOptions.GenerateTypedLists)
            {
                code.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            }
            else if (codeGenOptions.GenerateCollections)
            {
                code.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
            }

            code.Imports.Add(new CodeNamespaceImport("AntServiceStack.ProtoBuf"));
            
            // Set Clr namespace
            code.Name = codeGenOptions.ClrNamespace;

            //check AntServiceStack.ServiceHost
            CodeNamespaceImport ServiceHost = code.Imports.OfType<CodeNamespaceImport>().FirstOrDefault(
                        item =>
                        {
                            return item.Namespace == Constants.C_SERVICE_STACK_SERVICE_HOST_NAMESPACE;
                        });
            if (ServiceHost == null)
            {
                foreach (CodeTypeDeclaration type in code.Types)
                {
                    if (type.Name.Contains("Response"))
                    {
                        code.Imports.Add(new CodeNamespaceImport(Constants.C_SERVICE_STACK_SERVICE_HOST_NAMESPACE));
                        break;
                    }
                }
            }
        }
    }

}

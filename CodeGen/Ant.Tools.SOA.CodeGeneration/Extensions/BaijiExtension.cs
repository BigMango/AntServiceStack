using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using Newtonsoft.Json.Linq;
using Ant.Tools.SOA.CodeGeneration.Options;
using System.Text.RegularExpressions;

namespace Ant.Tools.SOA.CodeGeneration.Extensions
{
    /// <summary>
    /// Baiji Extension Class
    /// </summary>
    public static class BaijiExtension
    {
        #region Private Static Fields

        private static Dictionary<string, string> nullableTypeMapping;

        private static List<string> castableTypeList;

        private static Dictionary<string, string> clrTypeToSchemaTypeMapping;

        private static List<string> generatedTypes;

        private static Dictionary<string, string> specialPropertyNameToTypeNameMapping;

        private static Dictionary<string, CodeTypeReference> specialPropertyNameToCodeTypeMapping;

        private static CodeGenOptions codeGenOptions;

        private static CodeNamespace currentCodeNamespace;

        private static readonly string xsdNamespacePrefix = "soa.ant.com";

        private static CodeTypeDeclaration codeTypeDeclaration;

        private static IDictionary<string, CodeTypeDeclaration> codeTypeMap;

        private static List<CodeMemberProperty> codeMemberProperties;

        #endregion

        #region Initializing Methods
        
        static BaijiExtension()
        {
            BuildNullableTypeMapping();
            BuildCastableTypeList();
            BuildClrTypeToSchemaTypeMapping();
            if (specialPropertyNameToTypeNameMapping == null)
            {
                specialPropertyNameToTypeNameMapping = new Dictionary<string, string>();
                specialPropertyNameToCodeTypeMapping = new Dictionary<string, CodeTypeReference>();
            }
        }

        private static void BuildNullableTypeMapping()
        {
            if (nullableTypeMapping != null) return;
            nullableTypeMapping = new Dictionary<string, string>();
            nullableTypeMapping.Add("bool", "bool");
            nullableTypeMapping.Add("Boolean", "bool");
            nullableTypeMapping.Add("System.Boolean", "bool");   // bool
            nullableTypeMapping.Add("bool?", "bool?");
            nullableTypeMapping.Add("Boolean?", "bool?");
            nullableTypeMapping.Add("System.Boolean?", "bool?"); // bool?

            nullableTypeMapping.Add("byte", "byte");
            nullableTypeMapping.Add("Byte", "byte");
            nullableTypeMapping.Add("System.Byte", "byte");      // byte
            nullableTypeMapping.Add("byte?", "byte?");
            nullableTypeMapping.Add("Byte?", "byte?");
            nullableTypeMapping.Add("System.Byte?", "byte?");    // byte?

            nullableTypeMapping.Add("sbyte", "sbyte");
            nullableTypeMapping.Add("SByte", "sbyte");
            nullableTypeMapping.Add("System.SByte", "sbyte");    // sbyte
            nullableTypeMapping.Add("sbyte?", "sbyte?");
            nullableTypeMapping.Add("SByte?", "sbyte?");
            nullableTypeMapping.Add("System.SByte?", "sbyte?");  // sbyte?

            nullableTypeMapping.Add("short", "short");
            nullableTypeMapping.Add("Int16", "short");
            nullableTypeMapping.Add("System.Int16", "short");    // short
            nullableTypeMapping.Add("short?", "short?");
            nullableTypeMapping.Add("Int16?", "short?");
            nullableTypeMapping.Add("System.Int16?", "short?");  // short?

            nullableTypeMapping.Add("ushort", "ushort");
            nullableTypeMapping.Add("UInt16", "ushort");
            nullableTypeMapping.Add("System.UInt16", "ushort");  // ushort
            nullableTypeMapping.Add("ushort?", "ushort?");
            nullableTypeMapping.Add("UInt16?", "ushort?");
            nullableTypeMapping.Add("System.UInt16?", "ushort?"); // ushort?

            nullableTypeMapping.Add("int", "int");
            nullableTypeMapping.Add("Int32", "int");
            nullableTypeMapping.Add("System.Int32", "int");      // int
            nullableTypeMapping.Add("int?", "int?");
            nullableTypeMapping.Add("Int32?", "int?");
            nullableTypeMapping.Add("System.Int32?", "int?");    // int?

            nullableTypeMapping.Add("uint", "uint");
            nullableTypeMapping.Add("UInt32", "uint");
            nullableTypeMapping.Add("System.UInt32", "uint");    // uint
            nullableTypeMapping.Add("uint?", "uint?");
            nullableTypeMapping.Add("UInt32?", "uint?");
            nullableTypeMapping.Add("System.UInt32?", "uint?");  // uint?

            nullableTypeMapping.Add("long", "long");
            nullableTypeMapping.Add("Int64", "long");
            nullableTypeMapping.Add("System.Int64", "long");    // long
            nullableTypeMapping.Add("long?", "long?");
            nullableTypeMapping.Add("Int64?", "long?");
            nullableTypeMapping.Add("System.Int64?", "long?");   // long

            nullableTypeMapping.Add("ulong", "ulong");
            nullableTypeMapping.Add("UInt64", "ulong");
            nullableTypeMapping.Add("System.UInt64", "ulong");   // ulong
            nullableTypeMapping.Add("ulong?", "ulong?");
            nullableTypeMapping.Add("UInt64?", "ulong?");
            nullableTypeMapping.Add("System.UInt64?", "ulong?");  // ulong?

            nullableTypeMapping.Add("float", "float");
            nullableTypeMapping.Add("Single", "float");
            nullableTypeMapping.Add("System.Single", "float");   // float
            nullableTypeMapping.Add("float?", "float?");
            nullableTypeMapping.Add("Single?", "float?");
            nullableTypeMapping.Add("System.Single?", "float?");  // float?

            nullableTypeMapping.Add("double", "double");
            nullableTypeMapping.Add("Double", "double");
            nullableTypeMapping.Add("System.Double", "double");  // double
            nullableTypeMapping.Add("double?", "double?");
            nullableTypeMapping.Add("Double?", "double?");
            nullableTypeMapping.Add("System.Double?", "double?");  // double?

            nullableTypeMapping.Add("decimal", "decimal");
            nullableTypeMapping.Add("Decimal", "decimal");
            nullableTypeMapping.Add("System.Decimal", "decimal");  // decimal
            nullableTypeMapping.Add("decimal?", "decimal?");
            nullableTypeMapping.Add("Decimal?", "decimal?");
            nullableTypeMapping.Add("System.Decimal?", "decimal?"); // decimal?

            nullableTypeMapping.Add("DateTime", "System.DateTime");
            nullableTypeMapping.Add("System.DateTime", "System.DateTime"); // System.DateTime
            nullableTypeMapping.Add("DateTime?", "System.DateTime?");
            nullableTypeMapping.Add("System.DateTime?", "System.DateTime?"); // System.DateTime?

            nullableTypeMapping.Add("Guid", "System.Guid");
            nullableTypeMapping.Add("System.Guid", "System.Guid"); // System.Guid
            nullableTypeMapping.Add("Guid?", "System.Guid?");
            nullableTypeMapping.Add("System.Guid?", "System.Guid?"); // System.Guid?

            nullableTypeMapping.Add("TimeSpan", "System.TimeSpan");
            nullableTypeMapping.Add("System.TimeSpan", "System.TimeSpan"); // System.TimeSpan
            nullableTypeMapping.Add("TimeSpan?", "System.TimeSpan?");
            nullableTypeMapping.Add("System.TimeSpan?", "System.TimeSpan?"); // System.TimeSpan?
        }

        private static void BuildCastableTypeList()
        {
            if (castableTypeList != null) return;
            castableTypeList = new List<string>();
            castableTypeList.Add("bool");
            castableTypeList.Add("bool?");
            castableTypeList.Add("Boolean");
            castableTypeList.Add("Boolean?");
            castableTypeList.Add("System.Boolean");
            castableTypeList.Add("System.Boolean?"); // bool
            castableTypeList.Add("int");
            castableTypeList.Add("int?");
            castableTypeList.Add("Int32");
            castableTypeList.Add("Int32?");
            castableTypeList.Add("System.Int32");
            castableTypeList.Add("System.Int32?");   // int
            castableTypeList.Add("long");
            castableTypeList.Add("long?");
            castableTypeList.Add("Int64");
            castableTypeList.Add("Int64?");
            castableTypeList.Add("System.Int64");
            castableTypeList.Add("System.Int64?");   // long
            castableTypeList.Add("float");
            castableTypeList.Add("float?");
            castableTypeList.Add("Single");
            castableTypeList.Add("Single?");
            castableTypeList.Add("System.Single");
            castableTypeList.Add("System.Single?");  // float
            castableTypeList.Add("double");
            castableTypeList.Add("double?");
            castableTypeList.Add("Double");
            castableTypeList.Add("Double?");
            castableTypeList.Add("System.Double");
            castableTypeList.Add("System.Double?");  // double
            castableTypeList.Add("string");
            castableTypeList.Add("String");
            castableTypeList.Add("System.String");  // string
            castableTypeList.Add("DateTime");
            castableTypeList.Add("DateTime?");
            castableTypeList.Add("System.DateTime");
            castableTypeList.Add("System.DateTime?");// datetime
        }

        private static void BuildClrTypeToSchemaTypeMapping()
        {
            if (clrTypeToSchemaTypeMapping != null) return;
            clrTypeToSchemaTypeMapping = new Dictionary<string, string>();

            clrTypeToSchemaTypeMapping.Add("bool", "boolean");
            clrTypeToSchemaTypeMapping.Add("bool?", "boolean");
            clrTypeToSchemaTypeMapping.Add("System.Boolean", "boolean");

            clrTypeToSchemaTypeMapping.Add("byte", "int");
            clrTypeToSchemaTypeMapping.Add("byte?", "int");
            clrTypeToSchemaTypeMapping.Add("System.Byte", "int");
            clrTypeToSchemaTypeMapping.Add("sbyte", "int");
            clrTypeToSchemaTypeMapping.Add("sbyte?", "int");
            clrTypeToSchemaTypeMapping.Add("System.SByte", "int");

            clrTypeToSchemaTypeMapping.Add("short", "int");
            clrTypeToSchemaTypeMapping.Add("short?", "int");
            clrTypeToSchemaTypeMapping.Add("System.Int16", "int");
            clrTypeToSchemaTypeMapping.Add("ushort", "int");
            clrTypeToSchemaTypeMapping.Add("ushort?", "int");
            clrTypeToSchemaTypeMapping.Add("System.UInt16", "int");

            clrTypeToSchemaTypeMapping.Add("int", "int");
            clrTypeToSchemaTypeMapping.Add("int?", "int");
            clrTypeToSchemaTypeMapping.Add("System.Int32", "int");
            clrTypeToSchemaTypeMapping.Add("uint", "long");
            clrTypeToSchemaTypeMapping.Add("uint?", "long");
            clrTypeToSchemaTypeMapping.Add("System.UInt32", "long");

            clrTypeToSchemaTypeMapping.Add("long", "long");
            clrTypeToSchemaTypeMapping.Add("long?", "long");
            clrTypeToSchemaTypeMapping.Add("System.Int64", "long");
            clrTypeToSchemaTypeMapping.Add("ulong", "string");
            clrTypeToSchemaTypeMapping.Add("ulong?", "string");
            clrTypeToSchemaTypeMapping.Add("System.UInt64", "string");

            clrTypeToSchemaTypeMapping.Add("float", "float");
            clrTypeToSchemaTypeMapping.Add("float?", "float");
            clrTypeToSchemaTypeMapping.Add("System.Single", "float");

            clrTypeToSchemaTypeMapping.Add("double", "double");
            clrTypeToSchemaTypeMapping.Add("double?", "double");
            clrTypeToSchemaTypeMapping.Add("System.Double", "double");

            clrTypeToSchemaTypeMapping.Add("decimal", "string");
            clrTypeToSchemaTypeMapping.Add("decimal?", "string");
            clrTypeToSchemaTypeMapping.Add("System.Decimal", "string");

            clrTypeToSchemaTypeMapping.Add("string", "string");
            clrTypeToSchemaTypeMapping.Add("String", "string");
            clrTypeToSchemaTypeMapping.Add("System.String", "string");

            clrTypeToSchemaTypeMapping.Add("Uri", "string");
            clrTypeToSchemaTypeMapping.Add("System.Uri", "string");

            clrTypeToSchemaTypeMapping.Add("Guid", "string");
            clrTypeToSchemaTypeMapping.Add("Guid?", "string");
            clrTypeToSchemaTypeMapping.Add("System.Guid", "string");
            clrTypeToSchemaTypeMapping.Add("System.Guid?", "string");

            clrTypeToSchemaTypeMapping.Add("DateTime", "datetime");
            clrTypeToSchemaTypeMapping.Add("DateTime?", "datetime");
            clrTypeToSchemaTypeMapping.Add("System.DateTime", "datetime");
            clrTypeToSchemaTypeMapping.Add("System.DateTime?", "datetime");

            clrTypeToSchemaTypeMapping.Add("TimeSpan", "string");
            clrTypeToSchemaTypeMapping.Add("TimeSpan?", "string");
            clrTypeToSchemaTypeMapping.Add("System.TimeSpan", "string");
            clrTypeToSchemaTypeMapping.Add("System.TimeSpan?", "string");
        } 
        
        #endregion

        public static void ImplementsBaijiSerialization(this CodeCompileUnit codeCompileUnit, ICodeGeneratorContext codeGeneratorContext)
        {
            if (codeGeneratorContext.CodeGenOptions.EnableBaijiSerialization)
            {
                if (codeGeneratorContext.CodeTypeMap == null)
                    codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeCompileUnit);

                foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
                {
                    foreach (CodeTypeDeclaration type in codeNamespace.Types)
                    {
                        if (type.IsClass || type.IsStruct)
                        {
                            type.ImplementsBaijiSerialization(codeNamespace, codeGeneratorContext);
                        }
                    }
                }
            }
        }

        public static void ImplementsBaijiSerialization(this IEnumerable<CodeNamespace> codeNamespaces, ICodeGeneratorContext codeGeneratorContext)
        {
            if (codeGeneratorContext.CodeGenOptions.EnableBaijiSerialization)
            {
                if(codeGeneratorContext.CodeTypeMap == null)
                    codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespaces);

                foreach (CodeNamespace codeNamespace in codeNamespaces)
                {
                    foreach (CodeTypeDeclaration type in codeNamespace.Types)
                    {
                        if (type.IsClass || type.IsStruct)
                        {
                            type.ImplementsBaijiSerialization(codeNamespace, codeGeneratorContext);
                        }
                    }
                }
            }
        }

        public static void ImplementsBaijiSerialization(this CodeNamespace codeNamespace, ICodeGeneratorContext codeGeneratorContext)
        {
            if (codeGeneratorContext.CodeGenOptions.EnableBaijiSerialization)
            {
                if (codeGeneratorContext.CodeTypeMap == null)
                    codeGeneratorContext.CodeTypeMap = DataContractGenerator.BuildCodeTypeMap(codeNamespace);

                foreach (CodeTypeDeclaration type in codeNamespace.Types)
                {
                    if (type.IsClass || type.IsStruct)
                    {
                        type.ImplementsBaijiSerialization(codeNamespace, codeGeneratorContext);
                    }
                }
            }
        }

        /// <summary>
        /// Implements ISpecificRecord interface for current type
        /// </summary>
        /// <param name="codeTypeDeclaration"></param>
        /// <param name="clrNamespace">The namespace included current type</param>
        /// <param name="codeGeneratorContext">The code generator context</param>
        private static void ImplementsBaijiSerialization(this CodeTypeDeclaration codeTypeDeclaration, CodeNamespace clrNamespace, ICodeGeneratorContext codeGeneratorContext)
        {
            BaijiExtension.codeGenOptions = codeGeneratorContext.CodeGenOptions;
            BaijiExtension.codeTypeMap = codeGeneratorContext.CodeTypeMap;
            BaijiExtension.currentCodeNamespace = clrNamespace;
            BaijiExtension.codeTypeDeclaration = codeTypeDeclaration;
            specialPropertyNameToTypeNameMapping.Clear();
            specialPropertyNameToCodeTypeMapping.Clear();

            BaijiExtension.codeMemberProperties = new List<CodeMemberProperty>();
            foreach (CodeTypeMember member in codeTypeDeclaration.Members)
            {
                CodeMemberProperty property = member as CodeMemberProperty;
                if (property != null)
                {
                    if (property.Type.BaseType == "System.Runtime.Serialization.ExtensionDataObject") continue;
                    codeMemberProperties.Add((CodeMemberProperty)member);
                }
            }

            BaijiExtension.codeTypeDeclaration.BaseTypes.Add("ISpecificRecord");

            BaijiExtension.ImportNamespace(currentCodeNamespace.Imports, "AntServiceStack.Baiji.Specific");
            BaijiExtension.ImportNamespace(currentCodeNamespace.Imports, "System.Linq");

            BaijiExtension.ImplementsBaijiSchemaField();
            //BaijiExtension.ImplementsBaijiConstructors();
            BaijiExtension.ImplementsBaijiGetSchema();
            BaijiExtension.ImplementsBaijiGetByPos();
            BaijiExtension.ImplementsBaijiPutByPos();
            BaijiExtension.ImplementsBaijiGetByName();
            BaijiExtension.ImplementsBaijiPutByName();
            //BaijiExtension.OverrideBaijiEquals();
            //BaijiExtension.OverrideBaijiGetHashCode();
            //BaijiExtension.OverrideBaijiToString();
        }

        /// <summary>
        /// Resolve friendly name of CodeTypeReference 
        /// </summary>
        /// <param name="codeTypeReference">The CodeTypeReference</param>
        /// <returns>Friendly name of CodeTypeReference</returns>
        private static string ResolveTypeName(CodeTypeReference codeTypeReference)
        {
            string typeName;
            if (codeTypeReference.TypeArguments.Count == 0)
            {
                if (!nullableTypeMapping.TryGetValue(codeTypeReference.BaseType, out typeName))
                {
                    bool isGeneratedType = false;
                    string xsdTypeName = codeTypeReference.BaseType.ToLower();
                    if (codeGenOptions.OnlyUseDataContractSerializer && xsdTypeName.StartsWith(xsdNamespacePrefix))
                    {
                        string name = codeTypeReference.BaseType.Substring(codeTypeReference.BaseType.LastIndexOf('.') + 1);
                        
                        foreach (CodeTypeDeclaration codeType in codeTypeMap.Values)
                        {
                            if (string.Equals(name, codeType.Name))
                            {
                                bool isListType = false;
                                foreach (CodeTypeReference baseType in codeType.BaseTypes)
                                {
                                    if (baseType.BaseType.StartsWith("System.Collections.Generic.List"))
                                    {
                                        isListType = true;
                                        typeName = ResolveTypeName(baseType);
                                        break;
                                    }
                                }
                                if (!isListType) 
                                    typeName = codeType.Name;
                                isGeneratedType = true;
                                break;
                            }
                        }
                    }
                    if (!isGeneratedType)
                    {
                        typeName = codeTypeReference.BaseType;
                        if (typeName == "System.String")
                            return "string";
                    }
                }
                if (codeTypeReference.ArrayElementType != null)
                    typeName += "[]";
            }
            else if (codeTypeReference.TypeArguments.Count == 1) //such as: List`1
            {
                if (codeTypeReference.BaseType.StartsWith("System.Nullable"))
                {
                    typeName = ResolveTypeName(codeTypeReference.TypeArguments[0]) + "?";
                    //typeName = "System.Nullable<" + ResolveTypeName(typeTypeReference.TypeArguments[0]) + ">";
                }
                else
                {
                    typeName = codeTypeReference.BaseType.Substring(0, codeTypeReference.BaseType.LastIndexOf('`'));
                    typeName += "<" + ResolveTypeName(codeTypeReference.TypeArguments[0]) + ">";
                }
                //typeName = typeTypeReference.BaseType.Substring(0, typeTypeReference.BaseType.LastIndexOf('`'));
                //typeName += "<" + ResolveTypeName(typeTypeReference.TypeArguments[0]) + ">";
            }
            else
                throw new ArgumentException("Type:" + codeTypeReference.BaseType + " is not supported");
            return typeName;
        }

        /// <summary>
        /// Generate Baiji's SCHEMA field value
        /// </summary>
        /// <param name="typeDecl"></param>
        /// <returns></returns>
        private static string GenerateBaijiSchema(CodeTypeDeclaration typeDecl)
        {
            generatedTypes = new List<string>();
            string SchemaJson = GetTypeSpecificJson(typeDecl).ToString(Newtonsoft.Json.Formatting.None);
            return SchemaJson;
        }

        /// <summary>
        /// Get Specific Json of CodeTypeDeclaration
        /// </summary>
        /// <param name="typeDecl">The CodeTypeDeclaration</param>
        /// <returns>A JToken of specific json</returns>
        private static JToken GetTypeSpecificJson(CodeTypeDeclaration typeDecl)
        {
            if (generatedTypes.Contains(typeDecl.Name))
                return typeDecl.Name + "-Namespace." + typeDecl.Name;
            if (typeDecl.IsClass)
            {
                #region Class
                generatedTypes.Add(typeDecl.Name);
                JObject jsonObj = new JObject();
                jsonObj.Add("type", "record");
                jsonObj.Add("name", typeDecl.Name);
                jsonObj.Add("namespace", typeDecl.Name + "-Namespace");
                jsonObj.Add("doc", JValue.CreateNull());
                JArray fields = new JArray();
                foreach (CodeTypeMember member in typeDecl.Members)
                {
                    if (member is CodeMemberProperty)
                    {
                        CodeMemberProperty property = (CodeMemberProperty)member;
                        if (property.Type.BaseType == "System.Runtime.Serialization.ExtensionDataObject") continue;
                        JObject field = new JObject();
                        field.Add("name", property.Name);
                        field.Add("type", GetPropertySpecificJson(property));
                        fields.Add(field);
                    }
                }
                jsonObj.Add("fields", fields);

                return jsonObj;
                #endregion
            }
            else if (typeDecl.IsEnum)
            {
                #region Enumeration
                if (generatedTypes.Contains(typeDecl.Name))
                    return typeDecl.Name + "-Namespace." + typeDecl.Name;

                generatedTypes.Add(typeDecl.Name);
                JObject jsonObj = new JObject();
                jsonObj.Add("type", "enum");
                jsonObj.Add("name", typeDecl.Name);
                jsonObj.Add("namespace", typeDecl.Name + "-Namespace");
                jsonObj.Add("doc", JValue.CreateNull());
                JArray symbols = new JArray();
                foreach (CodeTypeMember symbol in typeDecl.Members)
                    symbols.Add(symbol.Name);
                jsonObj.Add("symbols", symbols);

                return jsonObj;
                #endregion
            }
            else
                throw new ArgumentException("Type: " + currentCodeNamespace.Name + typeDecl.Name + " is not supported! Only class and enum are supported");
        }

        /// <summary>
        /// Get Specific Json of CodeTypeReference
        /// </summary>
        /// <param name="typeRef">The CodeTypeReference</param>
        /// <returns>A JToken of specific json</returns>
        private static JToken GetTypeSpecificJson(string elementName, CodeTypeReference typeRef)
        {
            if (BaijiExtension.codeTypeMap.Keys.Contains(typeRef.BaseType)) // is custom type
            {
                if (generatedTypes.Contains(typeRef.BaseType))
                {
                    return typeRef.BaseType + "-Namespace." + typeRef.BaseType;
                }
                else
                {
                    return GetTypeSpecificJson(BaijiExtension.codeTypeMap[typeRef.BaseType]);
                }
            }
            else if (typeRef.BaseType.StartsWith("System.Nullable"))
            {
                JArray jsonArray = new JArray();
                jsonArray.Add(GetTypeSpecificJson(elementName, typeRef.TypeArguments[0]));
                jsonArray.Add("null");
                return jsonArray;
            }
            else // is base type
            {
                Type type = Type.GetType(typeRef.BaseType);
                string typeName = type == null ? typeRef.BaseType : type.FullName;
                string schemaTypeName;
                if (!clrTypeToSchemaTypeMapping.TryGetValue(typeName, out schemaTypeName))
                {
                    foreach (CodeTypeDeclaration codeType in codeTypeMap.Values)
                    {
                        if (typeName.EndsWith(codeType.Name))
                        {
                            return GetTypeSpecificJson(codeType);
                        }
                    }

                    throw new ArgumentException("Type: " + typeName + " is not supported! Element name: " + elementName);
                }
                if (!nullableTypeMapping.Keys.Contains(typeRef.BaseType))
                {
                    JArray jsonArray = new JArray();
                    jsonArray.Add(schemaTypeName);
                    jsonArray.Add("null");
                    return jsonArray;
                }
                else
                {
                    return new JValue(schemaTypeName);
                }
            }
        }

        /// <summary>
        /// Get Specific Json of CodeMemberProperty
        /// </summary>
        /// <param name="member">The CodeMemberProperty</param>
        /// <returns>A JToken of specific json</returns>
        private static JToken GetPropertySpecificJson(CodeMemberProperty member)
        {
            if (member.Type.ArrayElementType != null) // is an array
            {
                CodeTypeReference elementType = member.Type.ArrayElementType;
                if (elementType.BaseType == "byte" || //elementType.BaseType == "byte?" ||
                    elementType.BaseType == "Byte" || //elementType.BaseType == "Byte?" ||
                    elementType.BaseType == "System.Byte" )//|| elementType.BaseType == "System.Byte?")
                {
                    JArray typeArray = new JArray();
                    typeArray.Add("bytes");
                    typeArray.Add("null");
                    return typeArray;
                }
                else return GetArraySpecificJson(member.Name, elementType);
            }
            else if (member.Type.TypeArguments.Count == 1) // is list<> or nullable<>
            {
                if (member.Type.BaseType.StartsWith("System.Nullable"))
                {
                    JArray jsonArray = new JArray();
                    jsonArray.Add(GetTypeSpecificJson(member.Name, member.Type.TypeArguments[0]));
                    jsonArray.Add("null");

                    return jsonArray;
                }
                else
                    return GetArraySpecificJson(member.Name, member.Type.TypeArguments[0]);
            }
            else
            {
                #region Data contract special type (soa.ant.com.common.types.v1.StringListType)

                string xsdTypeName = member.Type.BaseType.ToLower();
                if (codeGenOptions.OnlyUseDataContractSerializer && xsdTypeName.ToLower().StartsWith(xsdNamespacePrefix))
                {
                    string name = member.Type.BaseType.Substring(member.Type.BaseType.LastIndexOf('.') + 1);
                    foreach (CodeTypeDeclaration codeType in codeTypeMap.Values)
                    {
                        if (string.Equals(name, codeType.Name))
                        {
                            CodeMemberProperty property = new CodeMemberProperty();
                            foreach (CodeTypeReference baseType in codeType.BaseTypes)
                            {
                                if (baseType.BaseType.StartsWith("System.Collections.Generic.List"))
                                {
                                    property.Type = baseType;
                                    property.Name = member.Name;
                                    if (!specialPropertyNameToTypeNameMapping.ContainsKey(member.Name))
                                    {
                                        specialPropertyNameToTypeNameMapping.Add(member.Name, codeType.Name);
                                        specialPropertyNameToCodeTypeMapping.Add(member.Name, baseType);
                                    }
                                    return GetPropertySpecificJson(property);
                                }
                            }
                            if (property.Type == null)
                            {
                                throw new ArgumentException("Type: " + member.Type.BaseType + " is not supported! Element name: " + member.Name);
                            }
                        }
                    } 
                }

                #endregion

                return GetTypeSpecificJson(member.Name, member.Type);
            }
        }

        /// <summary>
        /// Get Specific Json of CodeTypeReference
        /// </summary>
        /// <param name="elementTypeRef">The Array CodeTypeReference</param>
        /// <returns>A JToken of specific json</returns>
        private static JToken GetArraySpecificJson(string elementName, CodeTypeReference elementTypeRef)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("type", "array");
            jsonObject.Add("items", GetTypeSpecificJson(elementName, elementTypeRef));
            
            JArray jsonArray = new JArray();
            jsonArray.Add(jsonObject);
            jsonArray.Add("null");

            return jsonArray;
        }

        /// <summary>
        /// Get Specific Json of Dictionary (Dictionary is not supported yet)
        /// </summary>
        /// <param name="elementTypeRef">The Dictionary CodeTypeReference</param>
        /// <returns></returns>
        private static JToken GetDictionarySpecificJson(string elementName, CodeTypeReference elementTypeRef)
        {
            JArray jsonArray = new JArray();
            JObject jsonObj = new JObject();
            jsonObj.Add("type", "map");
            jsonObj.Add("values", GetTypeSpecificJson(elementName, elementTypeRef));
            jsonArray.Add(jsonObj);
            jsonArray.Add("null");
            return jsonArray;
        }

        /// <summary>
        /// Implements the SCHEMA field for current type
        /// </summary>
        private static void ImplementsBaijiSchemaField()
        {
            // SCHEMA Field
            string jsonString = GenerateBaijiSchema(codeTypeDeclaration);
            string[] pieces = jsonString.Split(new string[] { "-Namespace" }, StringSplitOptions.None);
            Stack<CodeBinaryOperatorExpression> addExpressions = new Stack<CodeBinaryOperatorExpression>();
            for (int i = 0; i < pieces.Length - 1; i++)
            {
                int typeNameIndex = pieces[i].LastIndexOf('"') + 1;
                string typeName = pieces[i].Substring(typeNameIndex);
                string right = "typeof(" + typeName + ").Namespace";

                CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression();
                expression.Left = new CodePrimitiveExpression(pieces[i].Substring(0, typeNameIndex));
                expression.Operator = CodeBinaryOperatorType.Add;
                expression.Right = new CodeSnippetExpression(right);
                addExpressions.Push(expression);
            }
            
            while (addExpressions.Count > 1)
            {
                CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression();
                expression.Right = addExpressions.Pop();
                expression.Operator = CodeBinaryOperatorType.Add;
                expression.Left = addExpressions.Pop();
                addExpressions.Push(expression);
            }
            CodeBinaryOperatorExpression final = new CodeBinaryOperatorExpression();
            final.Left = addExpressions.Pop();
            final.Operator = CodeBinaryOperatorType.Add;
            final.Right = new CodePrimitiveExpression(pieces[pieces.Length - 1]);

            CodeMemberField schemaField = new CodeMemberField("readonly AntServiceStack.Baiji.Schema.Schema", "SCHEMA");
            schemaField.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            schemaField.InitExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(new CodeTypeReference("AntServiceStack.Baiji.Schema.Schema")), "Parse", final);
            
            codeTypeDeclaration.Members.Add(schemaField);
        }

        /// <summary>
        /// Implements constructors for current type
        /// </summary>
        private static void ImplementsBaijiConstructors()
        {
            //public Constructor(NullArgs);
            CodeConstructor nullArgsConstructor = new CodeConstructor();
            nullArgsConstructor.Attributes = MemberAttributes.Public;
            nullArgsConstructor.Name = codeTypeDeclaration.Name;
            codeTypeDeclaration.Members.Add(nullArgsConstructor);

            //public Constructor(FullArgs);
            if (codeMemberProperties.Count > 0)
            {
                CodeConstructor fullArgsConstructor = new CodeConstructor();
                fullArgsConstructor.Attributes = MemberAttributes.Public;
                fullArgsConstructor.Name = codeTypeDeclaration.Name;
                StringBuilder builder = new StringBuilder();
                foreach (CodeMemberProperty property in codeMemberProperties)
                {
                    fullArgsConstructor.Parameters.Add(new CodeParameterDeclarationExpression(property.Type, property.Name));
                    builder.Append("            this." + property.Name + " = " + property.Name + ";\r\n");
                }
                if (builder.Length > 0) builder.Remove(builder.Length - 2, 2);
                fullArgsConstructor.Statements.Add(new CodeSnippetStatement(builder.ToString()));
                codeTypeDeclaration.Members.Add(fullArgsConstructor);
            }
        }

        /// <summary>
        /// Implements Method: Schema GetSchema(); for current type
        /// </summary>
        private static void ImplementsBaijiGetSchema()
        {
            CodeMemberMethod getSchema = new CodeMemberMethod();
            getSchema.Attributes = MemberAttributes.Public;
            getSchema.Name = "GetSchema";
            getSchema.ReturnType = new CodeTypeReference("AntServiceStack.Baiji.Schema.Schema");
            getSchema.ReturnType.BaseType = "AntServiceStack.Baiji.Schema.Schema";
            getSchema.Statements.Add(new CodeSnippetStatement("            return SCHEMA;"));
            codeTypeDeclaration.Members.Add(getSchema);
        }

        /// <summary>
        /// Implements Method: object Get(int fieldPos); for current type
        /// </summary>
        private static void ImplementsBaijiGetByPos()
        {
            CodeParameterDeclarationExpression fieldPos = new CodeParameterDeclarationExpression(typeof(int), "fieldPos");
            CodeMemberMethod getByPos = new CodeMemberMethod();
            getByPos.Attributes = MemberAttributes.Public;
            getByPos.Name = "Get";
            getByPos.ReturnType = new CodeTypeReference(typeof(object));
            getByPos.Parameters.Add(fieldPos);
            StringBuilder builder = new StringBuilder();
            builder.Append("            switch(fieldPos)\r\n            {\r\n");
            for (int i = 0; i < codeMemberProperties.Count; i++)
            {
                CodeMemberProperty property = codeMemberProperties[i];
                bool isNullableType = property.Type.BaseType.StartsWith("System.Nullable");
                string returnStatement;
                if (property.Type.ArrayElementType != null) // property is array type []
                {
                    #region if property is an array type []

                    string propertyElementTypeName = ResolveTypeName(property.Type.ArrayElementType);

                    if (propertyElementTypeName == "byte" || 
                        propertyElementTypeName == "Byte" || 
                        propertyElementTypeName == "System.Byte") // byte array
                    {
                        returnStatement = "return this." + property.Name + ";\r\n";
                    }
                    else if (castableTypeList.Contains(propertyElementTypeName) || //castable type
                        !clrTypeToSchemaTypeMapping.Keys.Contains(propertyElementTypeName)) //custom type
                    {
                        returnStatement = "return this." + property.Name + ";\r\n";
                    }
                    else // property element is non-castable type
                    {
                        string returnTypeName = clrTypeToSchemaTypeMapping[propertyElementTypeName];
                        string returnArgumentTypeName = returnTypeName;
                        string propertyArgumentTypeName = propertyElementTypeName;
                        if (isNullableType) // array element is nullable
                        {
                            if (returnTypeName == "string")
                            {
                                if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                    propertyArgumentTypeName += "?";
                            }
                            if (!propertyElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyElementTypeName))
                                propertyElementTypeName += "?";
                        }
                        returnStatement = "return AntServiceStack.Baiji.Specific.TypeConverter.ConvertToArray";
                        returnStatement += "<" + propertyArgumentTypeName + ", " + returnArgumentTypeName + ">";
                        returnStatement += "(this." + property.Name + ");\r\n";
                    }
                    
                    #endregion
                }
                else if (property.Type.TypeArguments.Count > 0) // property is list<> or nullable<>
                {
                    #region if property is a list or nullable type <>
                    
                    string propertyElementTypeName = ResolveTypeName(property.Type.TypeArguments[0]);
                    if (castableTypeList.Contains(propertyElementTypeName) ||  // property element is castable type
                        !clrTypeToSchemaTypeMapping.Keys.Contains(propertyElementTypeName)) // property element is custom type
                    {
                        returnStatement = "return this." + property.Name + ";\r\n";
                    }
                    else // property element is non-castable type
                    {
                        string returnTypeName = clrTypeToSchemaTypeMapping[propertyElementTypeName];
                        string returnArgumentTypeName = returnTypeName;
                        string propertyArgumentTypeName = propertyElementTypeName;

                        if (isNullableType) // property is nullable<> type
                        {
                            if (returnTypeName == "string")
                            {
                                if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                    propertyArgumentTypeName += "?";
                            }
                            if (!propertyElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyElementTypeName))
                                propertyElementTypeName += "?";

                            returnStatement = "return AntServiceStack.Baiji.Specific.TypeConverter.Convert";
                            returnStatement += "<" + propertyArgumentTypeName + ", " + returnArgumentTypeName + ">";
                            returnStatement += "(this." + property.Name + ");\r\n";
                        }
                        else // property is list<> type (list<> support only)
                        {
                            if ((property.Type.TypeArguments[0].BaseType.StartsWith("System.Nullable"))) // list element is nullable
                            {
                                if (returnTypeName == "string")
                                {
                                    if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                        propertyArgumentTypeName += "?";
                                }
                                if (!propertyElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyElementTypeName))
                                    propertyElementTypeName += "?";
                            }
                            returnStatement = "return AntServiceStack.Baiji.Specific.TypeConverter.ConvertToList";
                            returnStatement += "<" + propertyArgumentTypeName + ", " + returnArgumentTypeName + ">";
                            returnStatement += "(this." + property.Name + ");\r\n";
                        }
                    }

                    #endregion
                }
                else // other common type
                {
                    if (specialPropertyNameToTypeNameMapping.ContainsKey(property.Name))
                    {
                        #region Data contract special type (soa.ant.com.common.types.v1.StringListType)

                        CodeTypeReference baseType = specialPropertyNameToCodeTypeMapping[property.Name];
                        if (baseType.BaseType.StartsWith("System.Collections.Generic.List"))
                        {
                            string elementTypeName = ResolveTypeName(baseType.TypeArguments[0]);

                            //string paramConvertStatement;
                            if (castableTypeList.Contains(elementTypeName) || 
                                !clrTypeToSchemaTypeMapping.Keys.Contains(elementTypeName))
                            {
                                returnStatement = "return this." + property.Name + ";\r\n";
                            }
                            else
                            {
                                string returnTypeName = clrTypeToSchemaTypeMapping[elementTypeName];
                                string returnArgumentTypeName = returnTypeName;
                                string propertyArgumentTypeName = elementTypeName;

                                if ((baseType.TypeArguments[0].BaseType.StartsWith("System.Nullable"))) // list element is nullable
                                {
                                    if (returnTypeName == "string")
                                    {
                                        if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                            propertyArgumentTypeName += "?";
                                    }
                                }
                                returnStatement = "return AntServiceStack.Baiji.Specific.TypeConverter.ConvertToList";
                                returnStatement += "<" + propertyArgumentTypeName + ", " + returnArgumentTypeName + ">";
                                returnStatement += "(this." + property.Name + ");\r\n";
                            }
                        }
                        else
                            returnStatement = "return this." + property.Name + ";\r\n"; 

                        #endregion
                    }
                    else
                    {
                        #region Base types and common types
                        
                        string propertyTypeName = ResolveTypeName(property.Type);
                        string propertyArgumentTypeName = propertyTypeName;
                        if (castableTypeList.Contains(propertyTypeName) || 
                            !clrTypeToSchemaTypeMapping.Keys.Contains(propertyTypeName))
                        {
                            returnStatement = "return this." + property.Name + ";\r\n";
                        }
                        else
                        {
                            string returnTypeName = clrTypeToSchemaTypeMapping[propertyTypeName];
                            string returnArgumentType = returnTypeName;
                            if (isNullableType)
                            {
                                if (returnTypeName == "string")
                                {
                                    if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                        propertyArgumentTypeName += "?";
                                }
                                if (!propertyTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyTypeName))
                                    propertyTypeName += "?";
                            }
                            returnStatement = "return AntServiceStack.Baiji.Specific.TypeConverter.Convert";
                            returnStatement += "<" + propertyArgumentTypeName + ", " + returnArgumentType + ">";
                            returnStatement += "(this." + property.Name + ");\r\n";
                        }
                        
                        #endregion
                    }
                }
                
                builder.Append("                ");
                builder.Append("case " + i + ": " + returnStatement);
            }
            builder.Append("                ");
            builder.Append("default: throw new AntServiceStack.Baiji.Exceptions.BaijiRuntimeException(\"Bad index \" + fieldPos + \" in Get()\");");
            builder.Append("\r\n            }");
            getByPos.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(getByPos);
        }

        /// <summary>
        /// Implements Method: void Put(int fieldPos, object fieldValue); for current type
        /// </summary>
        private static void ImplementsBaijiPutByPos()
        {
            CodeParameterDeclarationExpression fieldPos = new CodeParameterDeclarationExpression(typeof(int), "fieldPos");
            CodeParameterDeclarationExpression fieldValue = new CodeParameterDeclarationExpression(typeof(object), "fieldValue");
            
            CodeMemberMethod putByPos = new CodeMemberMethod();
            putByPos.Attributes = MemberAttributes.Public;
            putByPos.Name = "Put";
            putByPos.Parameters.Add(fieldPos);
            putByPos.Parameters.Add(fieldValue);
            StringBuilder builder = new StringBuilder();
            builder.Append("            switch(fieldPos)\r\n            {\r\n");
            for (int i = 0; i < codeMemberProperties.Count; i++)
            {
                CodeMemberProperty property = codeMemberProperties[i];
                bool isNullableType = property.Type.BaseType.StartsWith("System.Nullable");
                string propertyTypeName = ResolveTypeName(property.Type);

                string convertStatement;
                if (property.Type.ArrayElementType != null)    // array type
                {
                    #region if property is an array type []
                    
                    string propertyElementTypeName = ResolveTypeName(property.Type.ArrayElementType);

                    if (propertyElementTypeName == "byte" ||
                        propertyElementTypeName == "Byte" ||
                        propertyElementTypeName == "System.Byte" || // byte array
                        castableTypeList.Contains(propertyElementTypeName) || // array element is castable clr array 
                        !clrTypeToSchemaTypeMapping.Keys.Contains(propertyElementTypeName)) // array element is custom clr array 
                    {
                        convertStatement = "((System.Collections.Generic.IEnumerable<" + propertyElementTypeName + ">)fieldValue).ToArray()";
                    }
                    else  // array element is non-castable clr array 
                    {
                        string paramElementTypeName = clrTypeToSchemaTypeMapping[propertyElementTypeName];
                        string paramArgumentTypeName = paramElementTypeName;
                        string propertyArgumentTypeName = propertyElementTypeName;
                        if (isNullableType) // property is nullable type
                        {
                            if (paramElementTypeName == "string")
                            {
                                if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                    propertyArgumentTypeName += "?";
                            }
                            if (!paramElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramElementTypeName))
                                paramElementTypeName += "?";
                        }
                        convertStatement = "AntServiceStack.Baiji.Specific.TypeConverter.ConvertToArray";
                        convertStatement += "<" + paramArgumentTypeName + ", " + propertyArgumentTypeName + ">";
                        convertStatement += "((System.Collections.Generic.IEnumerable<" + paramElementTypeName + ">)fieldValue)";
                    } 

                    #endregion
                }
                else if (property.Type.TypeArguments.Count > 0) // property is list<> or nullable<> type
                {
                    #region if property element is a list or a nullable type <>

                    string propertyElementTypeName = ResolveTypeName(property.Type.TypeArguments[0]);
                    if (castableTypeList.Contains(propertyElementTypeName) || // property element is castable base type
                        !clrTypeToSchemaTypeMapping.Keys.Contains(propertyElementTypeName)) // property element is custom type
                    {
                        convertStatement = "(" + propertyTypeName + ")fieldValue";
                    }
                    else // property element is non-castable base type
                    {
                        string paramElementTypeName = clrTypeToSchemaTypeMapping[propertyElementTypeName];
                        string paramArgumentTypeName = paramElementTypeName;
                        string propertyArgumentTypeName = propertyElementTypeName;
                        if (isNullableType) // property is nullable type
                        {
                            if (paramElementTypeName == "string")
                            {
                                if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                    propertyArgumentTypeName += "?";
                            }
                            if (!paramElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramElementTypeName))
                                paramElementTypeName += "?";

                            convertStatement = "AntServiceStack.Baiji.Specific.TypeConverter.Convert";
                            convertStatement += "<" + paramArgumentTypeName + ", " + propertyArgumentTypeName + ">";
                            convertStatement += "((" + paramElementTypeName + ")fieldValue)";
                        }
                        else // property is list<> type (list<> support only)
                        {
                            if ((property.Type.TypeArguments[0].BaseType.StartsWith("System.Nullable"))) // list element is nullable
                            {
                                if (paramElementTypeName == "string")
                                {
                                    if (!paramArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramArgumentTypeName))
                                        paramArgumentTypeName += "?";
                                }
                                if (!paramElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramElementTypeName))
                                    paramElementTypeName += "?";
                            }
                            convertStatement = "AntServiceStack.Baiji.Specific.TypeConverter.ConvertToList";
                            convertStatement += "<" + paramArgumentTypeName + ", " + propertyArgumentTypeName + ">";
                            convertStatement += "((System.Collections.Generic.IEnumerable<" + paramElementTypeName + ">)fieldValue)";
                        }
                    } 

                    #endregion
                }
                else  // property is common type
                {
                    if (specialPropertyNameToTypeNameMapping.ContainsKey(property.Name))
                    {
                        #region Data contract special type (soa.ant.com.common.types.v1.StringListType)
                        
                        CodeTypeReference baseType = specialPropertyNameToCodeTypeMapping[property.Name];
                        if (baseType.BaseType.StartsWith("System.Collections.Generic.List"))
                        {
                            string elementTypeName = ResolveTypeName(baseType.TypeArguments[0]);

                            string paramConvertStatement;
                            if (castableTypeList.Contains(elementTypeName) || 
                                !clrTypeToSchemaTypeMapping.Keys.Contains(elementTypeName))
                            {
                                paramConvertStatement = "(System.Collections.Generic.IEnumerable<" + elementTypeName + ">)fieldValue";
                            }
                            else
                            {
                                string paramElementTypeName = clrTypeToSchemaTypeMapping[elementTypeName];
                                string paramArgumentTypeName = paramElementTypeName;
                                string propertyArgumentTypeName = elementTypeName;
                                if ((baseType.TypeArguments[0].BaseType.StartsWith("System.Nullable"))) // list element is nullable
                                {
                                    if (paramElementTypeName == "string")
                                    {
                                        if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                            propertyArgumentTypeName += "?";
                                    }
                                    if (!paramElementTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramElementTypeName))
                                        paramElementTypeName += "?";
                                }

                                paramConvertStatement = "AntServiceStack.Baiji.Specific.TypeConverter.ConvertToList";
                                paramConvertStatement += "<" + paramArgumentTypeName + ", " + propertyArgumentTypeName + ">";
                                paramConvertStatement += "((System.Collections.Generic.IEnumerable<" + paramElementTypeName + ">)fieldValue)";
                            }
                            convertStatement = "new " + specialPropertyNameToTypeNameMapping[property.Name] + "();\r\n";
                            convertStatement += "                        ";
                            convertStatement += "this." + property.Name + ".AddRange(" + paramConvertStatement + ")";
                        }
                        else
                            convertStatement = "(" + specialPropertyNameToTypeNameMapping[property.Name] + ")fieldValue"; 

                        #endregion
                    }
                    else
                    {
                        #region Base types and common types

                        if (castableTypeList.Contains(propertyTypeName) || // property is castable base type
                                            !clrTypeToSchemaTypeMapping.Keys.Contains(propertyTypeName)) // property is custom type
                        {
                            convertStatement = "(" + propertyTypeName + ")fieldValue";
                        }
                        else // property is common non-castable clr type
                        {
                            string paramTypeName = clrTypeToSchemaTypeMapping[propertyTypeName];
                            string paramArgumentTypeName = paramTypeName;
                            string propertyArgumentTypeName = propertyTypeName;
                            if (isNullableType)
                            {
                                if (paramTypeName == "string")
                                {
                                    if (!propertyArgumentTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(propertyArgumentTypeName))
                                        propertyArgumentTypeName += "?";
                                }
                                if (!paramTypeName.EndsWith("?") && nullableTypeMapping.ContainsKey(paramTypeName))
                                    paramTypeName += "?";
                            }
                            convertStatement = "AntServiceStack.Baiji.Specific.TypeConverter.Convert";
                            convertStatement += "<" + paramArgumentTypeName + ", " + propertyArgumentTypeName + ">";
                            convertStatement += "((" + paramTypeName + ")fieldValue)";
                        } 
                        
                        #endregion
                    }
                }
                builder.Append("                ");
                builder.Append("case " + i + ": this." + codeMemberProperties[i].Name + " = " + convertStatement + "; break;\r\n");
            }
            builder.Append("                ");
            builder.Append("default: throw new AntServiceStack.Baiji.Exceptions.BaijiRuntimeException(\"Bad index \" + fieldPos + \" in Put()\");");
            builder.Append("\r\n            }");
            putByPos.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(putByPos);
        }

        /// <summary>
        /// Implements Method: object Get(string fieldName); for current type
        /// </summary>
        private static void ImplementsBaijiGetByName()
        {
            //object Get(string fieldName);
            CodeParameterDeclarationExpression fieldName = new CodeParameterDeclarationExpression(typeof(string), "fieldName");
            
            CodeMemberMethod getByName = new CodeMemberMethod();
            getByName.Attributes = MemberAttributes.Public;
            getByName.Name = "Get";
            getByName.ReturnType = new CodeTypeReference(typeof(object));
            getByName.Parameters.Add(fieldName);
            StringBuilder builder = new StringBuilder();
            builder.Append("            var recordSchema = GetSchema() as AntServiceStack.Baiji.Schema.RecordSchema;\r\n");
            builder.Append("            if (recordSchema == null)\r\n");
            builder.Append("            {\r\n");
            builder.Append("                return null;\r\n");
            builder.Append("            }\r\n");
            builder.Append("            AntServiceStack.Baiji.Schema.Field field;\r\n");
            builder.Append("            if (!recordSchema.TryGetField(fieldName, out field))\r\n");
            builder.Append("            {\r\n");
            builder.Append("                return null;\r\n");
            builder.Append("            }\r\n");
            builder.Append("            return Get(field.Pos);");
            getByName.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(getByName);
        }

        /// <summary>
        /// Implements Method: void Put(string fieldName, object fieldValue); for current type
        /// </summary>
        private static void ImplementsBaijiPutByName()
        {
            CodeParameterDeclarationExpression fieldName = new CodeParameterDeclarationExpression(typeof(string), "fieldName");
            CodeParameterDeclarationExpression fieldValue = new CodeParameterDeclarationExpression(typeof(object), "fieldValue");
            
            CodeMemberMethod putByName = new CodeMemberMethod();
            putByName.Attributes = MemberAttributes.Public;
            putByName.Name = "Put";
            putByName.Parameters.Add(fieldName);
            putByName.Parameters.Add(fieldValue);
            StringBuilder builder = new StringBuilder();
            builder.Append("            var recordSchema = GetSchema() as AntServiceStack.Baiji.Schema.RecordSchema;\r\n");
            builder.Append("            if (recordSchema == null)\r\n");
            builder.Append("            {\r\n");
            builder.Append("                return ;\r\n");
            builder.Append("            }\r\n");
            builder.Append("            AntServiceStack.Baiji.Schema.Field field;\r\n");
            builder.Append("            if (!recordSchema.TryGetField(fieldName, out field))\r\n");
            builder.Append("            {\r\n");
            builder.Append("                return ;\r\n");
            builder.Append("            }\r\n");
            builder.Append("            Put(field.Pos, fieldValue);");
            putByName.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(putByName);
        }

        /// <summary>
        /// Override Method: bool Equals(object that); for current type
        /// </summary>
        private static void OverrideBaijiEquals()
        {
            CodeMemberMethod equals = new CodeMemberMethod();
            equals.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            equals.Name = "Equals";
            equals.ReturnType = new CodeTypeReference(typeof(bool));
            equals.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "that"));
            
            StringBuilder builder = new StringBuilder();
            builder.Append("            var other = that as " + codeTypeDeclaration.Name + ";\r\n");
            builder.Append("            if (other == null) return false;\r\n");
            builder.Append("            if (ReferenceEquals(this, other)) return true;\r\n\r\n");
            builder.Append("            return \r\n");
            foreach (CodeMemberProperty property in codeMemberProperties)
            {
                if (property.Type.ArrayElementType != null) // array equals (byte[] is supported only)
                {
                    string arrayElementType = property.Type.ArrayElementType.BaseType;
                    builder.Append("                (" + property.Name + " == null ? other." + property.Name + " == null : " + property.Name + ".SequenceEqual(other." + property.Name + ")) &&\r\n");
                }
                else if (property.Type.TypeArguments.Count > 0 || property.Type.BaseType.StartsWith("List") || property.Type.BaseType.StartsWith("System.Collections.Generic.List")) // generic equals
                {
                    builder.Append("                AntServiceStack.Baiji.Utils.ObjectUtils.AreEqual(" + property.Name + ", other." + property.Name + ") &&\r\n");
                }
                else
                {
                    builder.Append("                Equals(" + property.Name + ", other." + property.Name + ") &&\r\n");
                }
            }
            if (codeMemberProperties.Count > 0)
            {
                builder.Remove(builder.Length - 4, 4);
            }
            else
            {
                builder.Remove(builder.Length - 2, 2);
                builder.Append("true");
            }
            builder.Append(";");
            equals.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(equals);
            ImportNamespace(currentCodeNamespace.Imports, "System.Linq");
        }

        /// <summary>
        /// Override Method: int GetHashCode(); for current type
        /// </summary>
        private static void OverrideBaijiGetHashCode()
        {
            CodeMemberMethod getHashCode = new CodeMemberMethod();
            getHashCode.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            getHashCode.Name = "GetHashCode";
            getHashCode.ReturnType = new CodeTypeReference(typeof(int));
            StringBuilder builder = new StringBuilder();
            builder.Append("            int result = 1;\r\n\r\n");
            foreach (CodeMemberProperty property in codeMemberProperties)
            {
                if (property.Type.ArrayElementType != null || property.Type.TypeArguments.Count > 0)
                {// array type, list type and nullable type
                    builder.Append("            result = (result * 397) ^ (" + property.Name + " == null ? 0 : AntServiceStack.Baiji.Utils.ObjectUtils.GetHashCode(" + property.Name + "));\r\n");
                }
                else
                {// other type, (base type, costom type)
                    if(nullableTypeMapping.Keys.Contains(property.Type.BaseType))
                        builder.Append("            result = (result * 397) ^ " + property.Name + ".GetHashCode();\r\n");
                    else
                        builder.Append("            result = (result * 397) ^ (" + property.Name + " == null ? 0 : " + property.Name + ".GetHashCode());\r\n");
                }
            }
            builder.Append("\r\n            return result;");
            getHashCode.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(getHashCode);
        }

        /// <summary>
        /// Override Method: string ToString(); for current type
        /// </summary>
        private static void OverrideBaijiToString()
        {
            //public override string ToString();
            CodeMemberMethod toString = new CodeMemberMethod();
            toString.Name = "ToString";
            toString.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            toString.ReturnType = new CodeTypeReference(typeof(string));
            StringBuilder builder = new StringBuilder();
            builder.Append("            var __sb = new System.Text.StringBuilder(\"" + codeTypeDeclaration.Name + "(\");\r\n\r\n");
            bool isFirst = true;
            foreach (CodeMemberProperty property in codeMemberProperties)
            {
                if (property.Type.ArrayElementType != null || 
                    property.Type.TypeArguments.Count > 0 ||
                    !nullableTypeMapping.ContainsKey(property.Type.BaseType))
                {
                    builder.Append("            if (" + property.Name + " != null)\r\n");
                    builder.Append("            {\r\n");
                    if (!isFirst)
                    {
                        builder.Append("                __sb.Append(\", \");\r\n");
                    }
                    builder.Append("                __sb.Append(\"" + property.Name + ":\" + " + property.Name + ");\r\n");
                    builder.Append("            }\r\n\r\n");
                }
                else
                {
                    if (!isFirst)
                    {
                        builder.Append("            __sb.Append(\", \");\r\n");
                    }
                    builder.Append("            __sb.Append(\"" + property.Name + ":\" + " + property.Name + ");\r\n\r\n");
                }
                isFirst = false;
            }
            builder.Append("\r\n            __sb.Append(\")\");\r\n");
            builder.Append("            return __sb.ToString();");
            toString.Statements.Add(new CodeSnippetStatement(builder.ToString()));
            codeTypeDeclaration.Members.Add(toString);
        }

        /// <summary>
        /// Import target namespace to CodeNamespaceImportCollection without duplicate
        /// </summary>
        public static void ImportNamespace(CodeNamespaceImportCollection imports, string targetNamespace)
        {
            CodeNamespaceImport targetImport = imports.OfType<CodeNamespaceImport>().FirstOrDefault(item => item.Namespace == targetNamespace);
            if (targetImport == null)
                imports.Add(new CodeNamespaceImport(targetNamespace));
        }

    }
}

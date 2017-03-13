using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using AntServiceStack.Common.Support;
using AntServiceStack.Text;

namespace AntServiceStackSwagger.Extentions
{
    public static class PlatformExtensions
    {
        private static Dictionary<string, List<Attribute>> propertyAttributesMap = new Dictionary<string, List<Attribute>>();
        private static Dictionary<Type, List<Attribute>> typeAttributesMap = new Dictionary<Type, List<Attribute>>();
        private static Dictionary<string, Type> GenericTypeCache = new Dictionary<string, Type>();
        private static readonly ConcurrentDictionary<Type, PlatformExtensions.ObjectDictionaryDefinition> toObjectMapCache = new ConcurrentDictionary<Type, PlatformExtensions.ObjectDictionaryDefinition>();
        private const string DataContract = "DataContractAttribute";

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        public static bool IsArray(this Type type)
        {
            return type.IsArray;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsGeneric(this Type type)
        {
            return type.IsGenericType;
        }

        public static Type BaseType(this Type type)
        {
            return type.BaseType;
        }

        public static Type ReflectedType(this PropertyInfo pi)
        {
            return pi.ReflectedType;
        }

        public static Type ReflectedType(this FieldInfo fi)
        {
            return fi.ReflectedType;
        }

        public static Type GenericTypeDefinition(this Type type)
        {
            return type.GetGenericTypeDefinition();
        }

        public static Type[] GetTypeInterfaces(this Type type)
        {
            return type.GetInterfaces();
        }

        public static Type[] GetTypeGenericArguments(this Type type)
        {
            return type.GetGenericArguments();
        }

        public static ConstructorInfo GetEmptyConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type)
        {
            return (IEnumerable<ConstructorInfo>)type.GetConstructors();
        }

        internal static PropertyInfo[] GetTypesPublicProperties(this Type subType)
        {
            return subType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        internal static PropertyInfo[] GetTypesProperties(this Type subType)
        {
            return subType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        public static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        public static MethodInfo GetMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName);
        }

        public static FieldInfo[] Fields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        public static PropertyInfo[] Properties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        public static FieldInfo[] GetAllFields(this Type type)
        {
            if (type.IsInterface())
                return TypeConstants.EmptyFieldInfoArray;
            return type.Fields();
        }

        public static FieldInfo[] GetPublicFields(this Type type)
        {
            if (type.IsInterface())
                return TypeConstants.EmptyFieldInfoArray;
            return ((ICollection<FieldInfo>)type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)).ToArray<FieldInfo>();
        }

        public static MemberInfo[] GetPublicMembers(this Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
        }

        public static MemberInfo[] GetAllPublicMembers(this Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public static MethodInfo GetStaticMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static MethodInfo GetInstanceMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static MethodInfo Method(this Delegate fn)
        {
            return fn.Method;
        }

        public static bool HasAttribute<T>(this Type type)
        {
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(type)).Any<object>((Func<object, bool>)(x => x.GetType() == typeof(T)));
        }

        public static bool HasAttribute<T>(this PropertyInfo pi)
        {
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(pi)).Any<object>((Func<object, bool>)(x => x.GetType() == typeof(T)));
        }

        public static bool HasAttribute<T>(this FieldInfo fi)
        {
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(fi)).Any<object>((Func<object, bool>)(x => x.GetType() == typeof(T)));
        }

        public static bool HasAttributeNamed(this Type type, string name)
        {
            string normalizedAttr = name.Replace("Attribute", "").ToLower();
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(type)).Any<object>((Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
        }

        public static bool HasAttributeNamed(this PropertyInfo pi, string name)
        {
            string normalizedAttr = name.Replace("Attribute", "").ToLower();
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(pi)).Any<object>((Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
        }

        public static bool HasAttributeNamed(this FieldInfo fi, string name)
        {
            string normalizedAttr = name.Replace("Attribute", "").ToLower();
            return ((IEnumerable<object>)PlatformExtensions.AllAttributes(fi)).Any<object>((Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
        }

        public static bool HasAttributeNamed(this MemberInfo mi, string name)
        {
            string normalizedAttr = name.Replace("Attribute", "").ToLower();
            return ((IEnumerable<object>)mi.AllAttributes()).Any<object>((Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
        }

        public static bool IsDto(this Type type)
        {
            if (type == (Type)null)
                return false;
            if (Env.IsMono)
                return ((IEnumerable<object>)type.GetCustomAttributes(true)).Any<object>((Func<object, bool>)(x => x.GetType().Name == "DataContractAttribute"));
            return type.HasAttribute<DataContractAttribute>();
        }

        public static MethodInfo PropertyGetMethod(this PropertyInfo pi, bool nonPublic = false)
        {
            return pi.GetGetMethod(nonPublic);
        }

        public static Type[] Interfaces(this Type type)
        {
            return type.GetInterfaces();
        }

        public static PropertyInfo[] AllProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void ClearRuntimeAttributes()
        {
            PlatformExtensions.propertyAttributesMap = new Dictionary<string, List<Attribute>>();
            PlatformExtensions.typeAttributesMap = new Dictionary<Type, List<Attribute>>();
        }

        internal static string UniqueKey(this PropertyInfo pi)
        {
            if (pi.DeclaringType == (Type)null)
                throw new ArgumentException("Property '{0}' has no DeclaringType".Fmt((object)pi.Name));
            return pi.DeclaringType.Namespace + "." + pi.DeclaringType.Name + "." + pi.Name;
        }

        public static Type AddAttributes(this Type type, params Attribute[] attrs)
        {
            List<Attribute> attributeList;
            if (!PlatformExtensions.typeAttributesMap.TryGetValue(type, out attributeList))
                PlatformExtensions.typeAttributesMap[type] = attributeList = new List<Attribute>();
            attributeList.AddRange((IEnumerable<Attribute>)attrs);
            return type;
        }

        /// <summary>
        /// Add a Property attribute at runtime.
        /// <para>Not threadsafe, should only add attributes on Startup.</para>
        /// </summary>
        public static PropertyInfo AddAttributes(this PropertyInfo propertyInfo, params Attribute[] attrs)
        {
            string key = propertyInfo.UniqueKey();
            List<Attribute> attributeList;
            if (!PlatformExtensions.propertyAttributesMap.TryGetValue(key, out attributeList))
                PlatformExtensions.propertyAttributesMap[key] = attributeList = new List<Attribute>();
            attributeList.AddRange((IEnumerable<Attribute>)attrs);
            return propertyInfo;
        }

        /// <summary>
        /// Add a Property attribute at runtime.
        /// <para>Not threadsafe, should only add attributes on Startup.</para>
        /// </summary>
        public static PropertyInfo ReplaceAttribute(this PropertyInfo propertyInfo, Attribute attr)
        {
            string key = propertyInfo.UniqueKey();
            List<Attribute> attributeList;
            if (!PlatformExtensions.propertyAttributesMap.TryGetValue(key, out attributeList))
                PlatformExtensions.propertyAttributesMap[key] = attributeList = new List<Attribute>();
            attributeList.RemoveAll((Predicate<Attribute>)(x => x.GetType() == attr.GetType()));
            attributeList.Add(attr);
            return propertyInfo;
        }

        public static List<TAttr> GetAttributes<TAttr>(this PropertyInfo propertyInfo)
        {
            List<Attribute> source;
            if (PlatformExtensions.propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out source))
                return source.OfType<TAttr>().ToList<TAttr>();
            return new List<TAttr>();
        }

        public static List<Attribute> GetAttributes(this PropertyInfo propertyInfo)
        {
            List<Attribute> source;
            if (PlatformExtensions.propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out source))
                return source.ToList<Attribute>();
            return new List<Attribute>();
        }

        public static List<Attribute> GetAttributes(this PropertyInfo propertyInfo, Type attrType)
        {
            List<Attribute> source;
            if (PlatformExtensions.propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out source))
                return source.Where<Attribute>((Func<Attribute, bool>)(x => attrType.IsInstanceOf(x.GetType()))).ToList<Attribute>();
            return new List<Attribute>();
        }

        public static object[] AllAttributes(this PropertyInfo propertyInfo)
        {
            object[] customAttributes = propertyInfo.GetCustomAttributes(true);
            List<Attribute> attributes = propertyInfo.GetAttributes();
            if (attributes.Count == 0)
                return customAttributes;
            attributes.AddRange(customAttributes.Cast<Attribute>());
            return attributes.Cast<object>().ToArray<object>();
        }

        public static List<TAttr> AllCustomAttributes<TAttr>(this MethodInfo methodInfo)
        {
            List<TAttr> source = new List<TAttr>();
            object[] customAttributes = methodInfo.GetCustomAttributes(true);
            foreach (var att in customAttributes)
            {
                if (att is TAttr)
                {
                    source.Add((TAttr)att);
                }
            }
            return source;
        }

        public static object[] AllAttributes(this PropertyInfo propertyInfo, Type attrType)
        {
            object[] customAttributes = propertyInfo.GetCustomAttributes(attrType, true);
            List<Attribute> attributes = propertyInfo.GetAttributes(attrType);
            if (attributes.Count == 0)
                return customAttributes;
            attributes.AddRange(customAttributes.Cast<Attribute>());
            return attributes.Cast<object>().ToArray<object>();
        }

        public static object[] AllAttributes(this ParameterInfo paramInfo)
        {
            return paramInfo.GetCustomAttributes(true);
        }

        public static object[] AllAttributes(this FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttributes(true);
        }

        public static object[] AllAttributes(this MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(true);
        }

        public static object[] AllAttributes(this ParameterInfo paramInfo, Type attrType)
        {
            return paramInfo.GetCustomAttributes(attrType, true);
        }

        public static object[] AllAttributes(this MemberInfo memberInfo, Type attrType)
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != (PropertyInfo)null)
                return PlatformExtensions.AllAttributes(propertyInfo, attrType);
            return memberInfo.GetCustomAttributes(attrType, true);
        }

        public static object[] AllAttributes(this FieldInfo fieldInfo, Type attrType)
        {
            return fieldInfo.GetCustomAttributes(attrType, true);
        }

        public static object[] AllAttributes(this Type type)
        {
            return ((IEnumerable<object>)type.GetCustomAttributes(true)).Union<object>((IEnumerable<object>)type.GetRuntimeAttributes((Type)null)).ToArray<object>();
        }

        public static object[] AllAttributes(this Type type, Type attrType)
        {
            return ((IEnumerable<object>)type.GetCustomAttributes(true)).Union<object>((IEnumerable<object>)type.GetRuntimeAttributes((Type)null)).ToArray<object>();
        }

        public static object[] AllAttributes(this Assembly assembly)
        {
            return ((ICollection<object>)assembly.GetCustomAttributes(true)).ToArray<object>();
        }

        public static TAttr[] AllAttributes<TAttr>(this ParameterInfo pi)
        {
            return pi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
        }

        public static TAttr[] AllAttributes<TAttr>(this MemberInfo mi)
        {
            return mi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
        }

        public static TAttr[] AllAttributes<TAttr>(this FieldInfo fi)
        {
            return PlatformExtensions.AllAttributes(fi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
        }

        public static TAttr[] AllAttributes<TAttr>(this PropertyInfo pi)
        {
            return PlatformExtensions.AllAttributes(pi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
        }

        private static IEnumerable<T> GetRuntimeAttributes<T>(this Type type)
        {
            List<Attribute> source;
            if (!PlatformExtensions.typeAttributesMap.TryGetValue(type, out source))
                return (IEnumerable<T>)new List<T>();
            return source.OfType<T>();
        }

        private static IEnumerable<Attribute> GetRuntimeAttributes(this Type type, Type attrType = null)
        {
            List<Attribute> source;
            if (!PlatformExtensions.typeAttributesMap.TryGetValue(type, out source))
                return (IEnumerable<Attribute>)new List<Attribute>();
            return source.Where<Attribute>((Func<Attribute, bool>)(x =>
            {
                if (!(attrType == (Type)null))
                    return attrType.IsInstanceOf(x.GetType());
                return true;
            }));
        }

        public static TAttr[] AllAttributes<TAttr>(this Type type)
        {
            return type.GetCustomAttributes(typeof(TAttr), true).OfType<TAttr>().Union<TAttr>(type.GetRuntimeAttributes<TAttr>()).ToArray<TAttr>();
        }

        public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : class
        {
            TAttr attr = (TAttr)((IEnumerable<object>)type.GetCustomAttributes(typeof(TAttr), true)).FirstOrDefault<object>();
            if ((object)attr != null)
                return attr;
            return type.GetRuntimeAttributes<TAttr>().FirstOrDefault<TAttr>();
        }

        public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo)
        {
            return ((IEnumerable<TAttribute>)memberInfo.AllAttributes<TAttribute>()).FirstOrDefault<TAttribute>();
        }

        public static TAttribute FirstAttribute<TAttribute>(this ParameterInfo paramInfo)
        {
            return ((IEnumerable<TAttribute>)paramInfo.AllAttributes<TAttribute>()).FirstOrDefault<TAttribute>();
        }

      

        public static Type FirstGenericTypeDefinition(this Type type)
        {
            Type type1 = type.FirstGenericType();
            if (!(type1 != (Type)null))
                return (Type)null;
            return type1.GetGenericTypeDefinition();
        }

        public static bool IsDynamic(this Assembly assembly)
        {
            try
            {
                return assembly is AssemblyBuilder || string.IsNullOrEmpty(assembly.Location);
            }
            catch (NotSupportedException ex)
            {
                return true;
            }
        }

        public static MethodInfo GetStaticMethod(this Type type, string methodName, Type[] types = null)
        {
            if (types != null)
                return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, (Binder)null, types, (ParameterModifier[])null);
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName, Type[] types = null)
        {
            if (types != null)
                return type.GetMethod(methodName, types);
            return type.GetMethod(methodName);
        }

        public static object InvokeMethod(this Delegate fn, object instance, object[] parameters = null)
        {
            return fn.Method.Invoke(instance, parameters ?? new object[0]);
        }

        public static FieldInfo GetPublicStaticField(this Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
        }

        public static Delegate MakeDelegate(this MethodInfo mi, Type delegateType, bool throwOnBindFailure = true)
        {
            return Delegate.CreateDelegate(delegateType, mi, throwOnBindFailure);
        }

        public static Type[] GenericTypeArguments(this Type type)
        {
            return type.GetGenericArguments();
        }

        public static ConstructorInfo[] DeclaredConstructors(this Type type)
        {
            return type.GetConstructors();
        }

        public static bool AssignableFrom(this Type type, Type fromType)
        {
            return type.IsAssignableFrom(fromType);
        }

        public static bool IsStandardClass(this Type type)
        {
            if (type.IsClass && !type.IsAbstract)
                return !type.IsInterface;
            return false;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.IsAbstract;
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName);
        }

        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            return type.GetField(fieldName);
        }

        public static FieldInfo[] GetWritableFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
        }

        public static MethodInfo SetMethod(this PropertyInfo pi, bool nonPublic = true)
        {
            return pi.GetSetMethod(nonPublic);
        }

        public static MethodInfo GetMethodInfo(this PropertyInfo pi, bool nonPublic = true)
        {
            return pi.GetGetMethod(nonPublic);
        }

        public static bool InstanceOfType(this Type type, object instance)
        {
            return type.IsInstanceOfType(instance);
        }

        public static bool IsAssignableFromType(this Type type, Type fromType)
        {
            return type.IsAssignableFrom(fromType);
        }

        public static bool IsClass(this Type type)
        {
            return type.IsClass;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsEnumFlags(this Type type)
        {
            if (type.IsEnum)
                return PlatformExtensions.FirstAttribute<FlagsAttribute>(type) != null;
            return false;
        }

        public static bool IsUnderlyingEnum(this Type type)
        {
            if (!type.IsEnum)
                return type.UnderlyingSystemType.IsEnum;
            return true;
        }

        public static MethodInfo[] GetMethodInfos(this Type type)
        {
            return type.GetMethods();
        }

        public static PropertyInfo[] GetPropertyInfos(this Type type)
        {
            return type.GetProperties();
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            return type.ContainsGenericParameters;
        }

        public static string GetDeclaringTypeName(this Type type)
        {
            if (type.DeclaringType != (Type)null)
                return type.DeclaringType.Name;
            if (type.ReflectedType != (Type)null)
                return type.ReflectedType.Name;
            return (string)null;
        }

        public static string GetDeclaringTypeName(this MemberInfo mi)
        {
            if (mi.DeclaringType != (Type)null)
                return mi.DeclaringType.Name;
            return mi.ReflectedType.Name;
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, methodInfo);
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target)
        {
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }

        public static Type ElementType(this Type type)
        {
            return type.GetElementType();
        }

        public static Type GetCollectionType(this Type type)
        {
            Type type1 = type.ElementType();
            if ((object)type1 != null)
                return type1;
            return ((IEnumerable<Type>)type.GetTypeGenericArguments()).FirstOrDefault<Type>();
        }

        public static Type GetCachedGenericType(this Type type, params Type[] argTypes)
        {
            if (!type.IsGenericTypeDefinition())
                throw new ArgumentException(type.FullName + " is not a Generic Type Definition");
            if (argTypes == null)
                argTypes = TypeConstants.EmptyTypeArray;
            StringBuilder sb = StringBuilderThreadStatic.Allocate().Append(type.FullName);
            foreach (Type argType in argTypes)
                sb.Append('|').Append(argType.FullName);
            string key = StringBuilderThreadStatic.ReturnAndFree(sb);
            Type type1;
            if (PlatformExtensions.GenericTypeCache.TryGetValue(key, out type1))
                return type1;
            Type type2 = type.MakeGenericType(argTypes);
            Dictionary<string, Type> genericTypeCache;
            Dictionary<string, Type> dictionary;
            do
            {
                genericTypeCache = PlatformExtensions.GenericTypeCache;
                dictionary = new Dictionary<string, Type>((IDictionary<string, Type>)PlatformExtensions.GenericTypeCache);
                dictionary[key] = type2;
            }
            while (Interlocked.CompareExchange<Dictionary<string, Type>>(ref PlatformExtensions.GenericTypeCache, dictionary, genericTypeCache) != genericTypeCache);
            return type2;
        }

        public static Dictionary<string, object> ToObjectDictionary(this object obj)
        {
            if (obj == null)
                return (Dictionary<string, object>)null;
            Dictionary<string, object> dictionary1 = obj as Dictionary<string, object>;
            if (dictionary1 != null)
                return dictionary1;
            IDictionary<string, object> dictionary2 = obj as IDictionary<string, object>;
            if (dictionary2 != null)
                return new Dictionary<string, object>(dictionary2);
            Type type = obj.GetType();
            PlatformExtensions.ObjectDictionaryDefinition dictionaryDefinition;
            if (!PlatformExtensions.toObjectMapCache.TryGetValue(type, out dictionaryDefinition))
                PlatformExtensions.toObjectMapCache[type] = dictionaryDefinition = PlatformExtensions.CreateObjectDictionaryDefinition(type);
            Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
            foreach (PlatformExtensions.ObjectDictionaryFieldDefinition field in dictionaryDefinition.Fields)
                dictionary3[field.Name] = field.GetValueFn(obj);
            return dictionary3;
        }

        public static object FromObjectDictionary(this Dictionary<string, object> values, Type type)
        {
            bool flag = type == typeof(Dictionary<string, object>);
            if (flag)
                return (object)flag;
            PlatformExtensions.ObjectDictionaryDefinition dictionaryDefinition;
            if (!PlatformExtensions.toObjectMapCache.TryGetValue(type, out dictionaryDefinition))
                PlatformExtensions.toObjectMapCache[type] = dictionaryDefinition = PlatformExtensions.CreateObjectDictionaryDefinition(type);
            object instance = type.CreateInstance();
            foreach (KeyValuePair<string, object> keyValuePair in values)
            {
                PlatformExtensions.ObjectDictionaryFieldDefinition dictionaryFieldDefinition;
                if (dictionaryDefinition.FieldsMap.TryGetValue(keyValuePair.Key, out dictionaryFieldDefinition) && keyValuePair.Value != null)
                    dictionaryFieldDefinition.SetValue(instance, keyValuePair.Value);
            }
            return instance;
        }

        public static object FromObjectDictionary<T>(this Dictionary<string, object> values)
        {
            return values.FromObjectDictionary(typeof(T));
        }

        private static PlatformExtensions.ObjectDictionaryDefinition CreateObjectDictionaryDefinition(Type type)
        {
            PlatformExtensions.ObjectDictionaryDefinition dictionaryDefinition = new PlatformExtensions.ObjectDictionaryDefinition()
            {
                Type = type
            };
            foreach (PropertyInfo serializableProperty in type.GetSerializableProperties())
                dictionaryDefinition.Add(serializableProperty.Name, new PlatformExtensions.ObjectDictionaryFieldDefinition()
                {
                    Name = serializableProperty.Name,
                    Type = serializableProperty.PropertyType,
                    GetValueFn = serializableProperty.GetPropertyGetterFn(),
                    SetValueFn = serializableProperty.GetPropertySetterFn()
                });
            if (JsConfig.IncludePublicFields)
            {
                foreach (FieldInfo serializableField in type.GetSerializableFields())
                    dictionaryDefinition.Add(serializableField.Name, new PlatformExtensions.ObjectDictionaryFieldDefinition()
                    {
                        Name = serializableField.Name,
                        Type = serializableField.FieldType,
                        GetValueFn = serializableField.GetFieldGetterFn(),
                        SetValueFn = serializableField.GetFieldSetterFn()
                    });
            }
            return dictionaryDefinition;
        }

        public static Dictionary<string, object> ToSafePartialObjectDictionary<T>(this T instance)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Dictionary<string, object> objectDictionary = ((object)instance).ToObjectDictionary();
            if (objectDictionary != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in objectDictionary)
                {
                    Type type = keyValuePair.Value != null ? keyValuePair.Value.GetType() : (Type)null;
                    dictionary[keyValuePair.Key] = type == (Type)null || !type.IsClass() || type == typeof(string) ? keyValuePair.Value : (HasCircularReferences(keyValuePair.Value) ? (object)keyValuePair.Value.ToString() : (!(keyValuePair.Value is IEnumerable) ? (object)keyValuePair.Value.ToSafePartialObjectDictionary<object>() : keyValuePair.Value));
                }
            }
            return dictionary;
        }
        public static bool HasCircularReferences(object value)
        {
            return HasCircularReferences(value, (Stack<object>)null);
        }
        public static bool HasCircularReferences(object value, Stack<object> parentValues)
        {
            Type type = value != null ? value.GetType() : (Type)null;
            if (type == (Type)null || !type.IsClass() || value is string)
                return false;
            if (parentValues == null)
            {
                parentValues = new Stack<object>();
                parentValues.Push(value);
            }
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (object obj in enumerable)
                {
                    if (HasCircularReferences(obj, parentValues))
                        return true;
                }
            }
            else
            {
                foreach (PropertyInfo serializableProperty in type.GetSerializableProperties())
                {
                    if (serializableProperty.GetIndexParameters().Length == 0)
                    {
                        MethodInfo method = serializableProperty.PropertyGetMethod(false);
                        object obj = method != (MethodInfo)null ? method.Invoke(value, (object[])null) : (object)null;
                        if (obj != null)
                        {
                            if (parentValues.Contains(obj))
                                return true;
                            parentValues.Push(obj);
                            if (HasCircularReferences(obj, parentValues))
                                return true;
                            parentValues.Pop();
                        }
                    }
                }
            }
            return false;
        }
        internal class ObjectDictionaryDefinition
        {
            public readonly List<PlatformExtensions.ObjectDictionaryFieldDefinition> Fields = new List<PlatformExtensions.ObjectDictionaryFieldDefinition>();
            public readonly Dictionary<string, PlatformExtensions.ObjectDictionaryFieldDefinition> FieldsMap = new Dictionary<string, PlatformExtensions.ObjectDictionaryFieldDefinition>();
            public Type Type;

            public void Add(string name, PlatformExtensions.ObjectDictionaryFieldDefinition fieldDef)
            {
                this.Fields.Add(fieldDef);
                this.FieldsMap[name] = fieldDef;
            }
        }

        internal class ObjectDictionaryFieldDefinition
        {
            public string Name;
            public Type Type;
            public PropertyGetterDelegate GetValueFn;
            public PropertySetterDelegate SetValueFn;
            public Type ConvertType;
            public PropertyGetterDelegate ConvertValueFn;

            public void SetValue(object instance, object value)
            {
                if (this.SetValueFn == null)
                    return;
                if (!this.Type.InstanceOfType(value))
                {
                    lock (this)
                    {
                        if (this.ConvertType == (Type)null)
                        {
                            this.ConvertType = value.GetType();
                            this.ConvertValueFn = TypeConverter.CreateTypeConverter(this.ConvertType, this.Type);
                        }
                    }
                    value = !this.ConvertType.InstanceOfType(value) ? TypeConverter.CreateTypeConverter(value.GetType(), this.Type)(value) : this.ConvertValueFn(value);
                }
                this.SetValueFn(instance, value);
            }
        }

        internal static class StringBuilderThreadStatic
        {
            [ThreadStatic]
            private static StringBuilder cache;

            public static StringBuilder Allocate()
            {
                StringBuilder cache = StringBuilderThreadStatic.cache;
                if (cache == null)
                    return new StringBuilder();
                cache.Length = 0;
                StringBuilderThreadStatic.cache = (StringBuilder)null;
                return cache;
            }

            public static void Free(StringBuilder sb)
            {
                StringBuilderThreadStatic.cache = sb;
            }

            public static string ReturnAndFree(StringBuilder sb)
            {
                string str = sb.ToString();
                StringBuilderThreadStatic.cache = sb;
                return str;
            }
        }

       
    }
    internal static class FieldInvoker
    {
        public static PropertySetterDelegate GetFieldSetterFn(this FieldInfo fieldInfo)
        {
            return new PropertySetterDelegate(fieldInfo.SetValue);
        }

        public static PropertyGetterDelegate GetFieldGetterFn(this FieldInfo fieldInfo)
        {
            return new PropertyGetterDelegate(fieldInfo.GetValue);
        }

        
    }
}

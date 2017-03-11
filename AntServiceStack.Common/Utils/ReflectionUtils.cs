using System;
using System.Collections.Generic;
#if NETFX_CORE
using System.Collections.Concurrent;
#endif
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AntServiceStack.Common.Support;
using Freeway.Logging;
using AntServiceStack.Net30.Collections.Concurrent;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Globalization;
using System.Data;
using AntServiceStack.Common.Configuration;

namespace AntServiceStack.Common.Utils
{
    public class ReflectionUtils
    {
        private const string LogPopulateExceptionAsWarningSettingKey = "SOA.LogPopulateExceptionAsWarning";

        private static readonly ILog logger = LogManager.GetLogger(typeof(ReflectionUtils));
        internal static readonly bool LogPopulateExceptionAsWarning;

        private static readonly Dictionary<Type, Func<object>> objectFactoryCache = new Dictionary<Type, Func<object>>();

        static ReflectionUtils()
        {
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(LogPopulateExceptionAsWarningSettingKey), out LogPopulateExceptionAsWarning))
                LogPopulateExceptionAsWarning = true;

            RegisterPopulateType<DataTable>(() => new DataTable("SampleTable"));
        }

        private static void Log(string title, Exception ex, Dictionary<string, string> tags)
        {
            if (LogPopulateExceptionAsWarning)
                logger.Warn(title, ex, tags);
            else
                logger.Info(title, ex, tags);
        }

        private static void Log(string title, Dictionary<string, string> tags)
        {
            if (LogPopulateExceptionAsWarning)
                logger.Warn(title, tags);
            else
                logger.Info(title, tags);
        }

        public static void RegisterPopulateType<T>(Func<T> factory)
        {
            objectFactoryCache[typeof(T)] = () => factory();
        }

        /// <summary>
        /// Populate an object with Example data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object PopulateObject(object obj)
        {
            try
            {
                if (obj == null) return null;

                var type = obj.GetType();
                Func<object> factory;
                if (objectFactoryCache.TryGetValue(type, out factory))
                    return factory();

                if (type.IsArray() || type.IsValueType() || type.IsGeneric())
                {
                    var value = CreateDefaultValue(type, new Dictionary<Type, int>(20));
                    return value;
                }

                return PopulateObjectInternal(obj, new Dictionary<Type, int>(20));
            }
            catch (Exception ex)
            {
                var message = "Error occurred while populating object. Type: " + obj.GetType().FullName;
                Log(message, ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300087" } });
                return obj;
            }
        }

        /// <summary>
        /// Populates the object with example data.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recursionInfo">Tracks how deeply nested we are</param>
        /// <returns></returns>
        private static object PopulateObjectInternal(object obj, Dictionary<Type, int> recursionInfo)
        {
            if (obj == null) return null;
            
            var type = obj.GetType();
            Func<object> factory;
            if (objectFactoryCache.TryGetValue(type, out factory))
                return factory();

            if (obj is string) return obj; // prevents it from dropping into the char[] Chars property.  Sheesh
            if (type.IsEnum) return obj;

            // If we have hit our recursion limit for this type, then return null
            int recurseLevel; // will get set to 0 if TryGetValue() fails
            recursionInfo.TryGetValue(type, out recurseLevel);
            if (recurseLevel > MaxRecursionLevelForDefaultValues) return null;
            
            recursionInfo[type] = recurseLevel + 1; // increase recursion level for this type
            try
            {
                var members = type.GetPublicMembers();
                foreach (var info in members)
                {
                    var fieldInfo = info as FieldInfo;
                    var propertyInfo = info as PropertyInfo;
                    if (fieldInfo != null || propertyInfo != null)
                    {
                        var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        var currentValue = GetValue(fieldInfo, propertyInfo, obj);
                        if (currentValue == null || memberType.IsValueType)
                        {
                            var newValue = CreateDefaultValue(memberType, recursionInfo);
                            SetValue(fieldInfo, propertyInfo, obj, newValue);
                        }
                        else
                        {
                            bool isCollection = currentValue is System.Collections.ICollection;
                            if (!isCollection)
                            {
                                var newValue = PopulateObjectInternal(currentValue, recursionInfo);
                                SetValue(fieldInfo, propertyInfo, obj, newValue);
                            }
                            else
                            {
                                if (memberType.IsArray)
                                {
                                    var array = PopulateArray((Array)currentValue, memberType.GetElementType(), recursionInfo);
                                    SetValue(fieldInfo, propertyInfo, obj, array);
                                }
                                else
                                {
                                    var collectionValue = currentValue as System.Collections.ICollection;
                                    SetGenericCollection(collectionValue, recursionInfo);
                                }
                            }
                        }
                    }
                }
                return obj;
            }
            finally
            {
                recursionInfo[type] = recurseLevel;
            }
        }

        private static readonly Dictionary<Type, object> DefaultValueTypes
            = new Dictionary<Type, object>();

        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType()) return null;

            object defaultValue;
            lock (DefaultValueTypes)
            {
                if (!DefaultValueTypes.TryGetValue(type, out defaultValue))
                {
                    defaultValue = Activator.CreateInstance(type);
                    DefaultValueTypes[type] = defaultValue;
                }
            }

            return defaultValue;
        }

        private static readonly ConcurrentDictionary<string, AssignmentDefinition> AssignmentDefinitionCache
            = new ConcurrentDictionary<string, AssignmentDefinition>();

        public static AssignmentDefinition GetAssignmentDefinition(Type toType, Type fromType)
        {
            var cacheKey = toType.FullName + "<" + fromType.FullName;

            return AssignmentDefinitionCache.GetOrAdd(cacheKey, delegate
            {

                var definition = new AssignmentDefinition
                {
                    ToType = toType,
                    FromType = fromType,
                };

                var readMap = GetMembers(fromType, isReadable: true);
                var writeMap = GetMembers(toType, isReadable: false);

                foreach (var assignmentMember in readMap)
                {
                    AssignmentMember writeMember;
                    if (writeMap.TryGetValue(assignmentMember.Key, out writeMember))
                    {
                        definition.AddMatch(assignmentMember.Key, assignmentMember.Value, writeMember);
                    }
                }

                return definition;
            });
        }

        private static Dictionary<string, AssignmentMember> GetMembers(Type type, bool isReadable)
        {
            var map = new Dictionary<string, AssignmentMember>();

            var members = type.GetAllPublicMembers();
            foreach (var info in members)
            {
                if (info.DeclaringType == typeof(object)) continue;

                var propertyInfo = info as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (isReadable)
                    {
                        if (propertyInfo.CanRead)
                        {
                            map[info.Name] = new AssignmentMember(propertyInfo.PropertyType, propertyInfo);
                            continue;
                        }
                    }
                    else
                    {
                        if (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null)
                        {
                            map[info.Name] = new AssignmentMember(propertyInfo.PropertyType, propertyInfo);
                            continue;
                        }
                    }
                }

                var fieldInfo = info as FieldInfo;
                if (fieldInfo != null)
                {
                    map[info.Name] = new AssignmentMember(fieldInfo.FieldType, fieldInfo);
                    continue;
                }

                var methodInfo = info as MethodInfo;
                if (methodInfo != null)
                {
                    var parameterInfos = methodInfo.GetParameters();
                    if (isReadable)
                    {
                        if (parameterInfos.Length == 0)
                        {
                            var name = info.Name.StartsWith("get_") ? info.Name.Substring(4) : info.Name;
                            if (!map.ContainsKey(name))
                            {
                                map[name] = new AssignmentMember(methodInfo.ReturnType, methodInfo);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (parameterInfos.Length == 1 && methodInfo.ReturnType == typeof(void))
                        {
                            var name = info.Name.StartsWith("set_") ? info.Name.Substring(4) : info.Name;
                            if (!map.ContainsKey(name))
                            {
                                map[name] = new AssignmentMember(parameterInfos[0].ParameterType, methodInfo);
                                continue;
                            }
                        }
                    }
                }
            }

            return map;
        }

        public static To PopulateObject<To, From>(To to, From from)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.Populate(to, from);

            return to;
        }

        public static To PopulateWithNonDefaultValues<To, From>(To to, From from)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateWithNonDefaultValues(to, from);

            return to;
        }

        public static To PopulateFromPropertiesWithAttribute<To, From>(To to, From from,
            Type attributeType)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateFromPropertiesWithAttribute(to, from, attributeType);

            return to;
        }

        public static void SetProperty(object obj, PropertyInfo propertyInfo, object value)
        {
            if (!propertyInfo.CanWrite)
            {
                Log(string.Format("Attempted to set read only property '{0}'", propertyInfo.Name),
                    new Dictionary<string, string>() { { "ErrorCode", "FXD300037" } });
                return;
            }

            var propertySetMetodInfo = propertyInfo.SetMethod();
            if (propertySetMetodInfo != null)
            {
                propertySetMetodInfo.Invoke(obj, new[] { value });
            }
        }

        public static object GetProperty(object obj, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || !propertyInfo.CanRead)
                return null;

            var getMethod = propertyInfo.GetMethodInfo();
            return getMethod != null ? getMethod.Invoke(obj, new object[0]) : null;
        }

        public static void SetValue(FieldInfo fieldInfo, PropertyInfo propertyInfo, object obj, object value)
        {
            try
            {
                if (IsUnsettableValue(fieldInfo, propertyInfo)) return;
                if (fieldInfo != null && !fieldInfo.IsLiteral)
                {
                    fieldInfo.SetValue(obj, value);
                }
                else
                {
                    SetProperty(obj, propertyInfo, value);
                }
            }
            catch (Exception ex)
            {
                var name = (fieldInfo != null) ? fieldInfo.Name : propertyInfo.Name;
                Log(string.Format("Could not set member: {0}. ", name), ex,
                    new Dictionary<string, string>() { { "ErrorCode", "FXD300038" } });
            }
        }

        public static object GetValue(FieldInfo fieldInfo, PropertyInfo propertyInfo, object obj)
        {
            try
            {
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(obj);
                }
                else
                {
                    if (propertyInfo.GetIndexParameters().Length != 0)
                        return null;

                    return propertyInfo.GetValue(obj, null);
                }
            }
            catch (Exception ex)
            {
                var name = (fieldInfo != null) ? fieldInfo.Name : propertyInfo.Name;
                Log(string.Format("Could not get member: {0}. ", name), ex,
                    new Dictionary<string, string>() { { "ErrorCode", "FXD300038" } });
                return null;
            }
        }

        public static bool IsUnsettableValue(FieldInfo fieldInfo, PropertyInfo propertyInfo)
        {
#if NETFX_CORE
            if (propertyInfo != null)
            {
                // Properties on non-user defined classes should not be set
                // Currently we define those properties as properties declared on
                // types defined in mscorlib

                if (propertyInfo.DeclaringType.AssemblyQualifiedName.Equals(typeof(object).AssemblyQualifiedName))
                {
                    return true;
                }
            }
#else
            if (propertyInfo != null && propertyInfo.ReflectedType != null)
            {
                // Properties on non-user defined classes should not be set
                // Currently we define those properties as properties declared on
                // types defined in mscorlib

                if (propertyInfo.DeclaringType.Assembly == typeof(object).Assembly)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static object[] CreateDefaultValues(IEnumerable<Type> types, Dictionary<Type, int> recursionInfo)
        {
            var values = new List<object>();
            foreach (var type in types)
            {
                values.Add(CreateDefaultValue(type, recursionInfo));
            }
            return values.ToArray();
        }

        private const int MaxRecursionLevelForDefaultValues = 2; // do not nest a single type more than this deep.

        public static object CreateDefaultValue(Type type, Dictionary<Type, int> recursionInfo)
        {
            Func<object> factory;
            if (objectFactoryCache.TryGetValue(type, out factory))
                return factory();

            if (type == typeof(string))
            {
                return type.Name;
            }

            if (type.IsEnum())
            {
#if SILVERLIGHT4 || WINDOWS_PHONE
                return Enum.ToObject(type, 0);
#else
                return Enum.GetValues(type).GetValue(0);
#endif
            }

            //when using KeyValuePair<TKey, TValue>, TKey must be non-default to stuff in a Dictionary
            if (type.IsGeneric() && type.GenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var genericTypes = type.GenericTypeArguments();
                var valueType = Activator.CreateInstance(type, CreateDefaultValue(genericTypes[0], recursionInfo), CreateDefaultValue(genericTypes[1], recursionInfo));
                return PopulateObjectInternal(valueType, recursionInfo);
            }

            if (type.IsValueType())
            {
                if (type.IsPrimitive)
                    return Activator.CreateInstance(type);

                var valueType = Activator.CreateInstance(type);
                return PopulateObjectInternal(valueType, recursionInfo);
            }

            if (type.IsArray)
            {
                return PopulateArray(type, recursionInfo);
            }

            if (type.GetInterfaces().Contains(typeof(ICollection)))
            {
                var genericTypes = type.GenericTypeArguments();
                if (genericTypes.Length == 1)
                {
                    var value = (ICollection)Activator.CreateInstance(type);
                    SetGenericCollection(value, recursionInfo);
                    return value;
                }

                if (genericTypes.Length == 2 && type.GetInterfaces().Contains(typeof(IDictionary<,>)))
                {
                    var valueType = Activator.CreateInstance(type, CreateDefaultValue(genericTypes[0], recursionInfo), CreateDefaultValue(genericTypes[1], recursionInfo));
                    return PopulateObjectInternal(valueType, recursionInfo);
                }
            }

            var constructorInfo = type.GetEmptyConstructor();
            var hasEmptyConstructor = constructorInfo != null;

            if (hasEmptyConstructor)
            {
                var value = constructorInfo.Invoke(new object[0]);

#if !SILVERLIGHT && !MONOTOUCH && !XBOX

                var genericCollectionType = GetGenericCollectionType(type);
                if (genericCollectionType != null)
                {
                    SetGenericCollection(genericCollectionType, value, recursionInfo);
                }
#endif

                //when the object might have nested properties such as enums with non-0 values, etc
                return PopulateObjectInternal(value, recursionInfo);
            }
            return null;
        }

        private static Type GetGenericCollectionType(Type type)
        {
#if NETFX_CORE
            var genericCollectionType =
                type.GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof (ICollection<>));
#elif WINDOWS_PHONE || SILVERLIGHT
            var genericCollectionType =
                type.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (ICollection<>));
#else
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                return type;

            var genericCollectionType = type.FindInterfaces((t, critera) =>
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>), null).FirstOrDefault();
#endif

            return genericCollectionType;
        }

        public static void SetGenericCollection(Type realisedListType, object genericObj, Dictionary<Type, int> recursionInfo)
        {
            var genericType = GetGenericCollectionType(realisedListType);
            var args = genericType.GenericTypeArguments();
            if (args.Length != 1)
            {
                Log(string.Format("Found a generic list that does not take one generic argument: {0}", realisedListType),
                    new Dictionary<string, string>() { { "ErrorCode", "FXD300039" } });

                return;
            }

            var methodInfo = realisedListType.GetMethodInfo("Add");
            if (methodInfo != null)
            {
                var argValues = CreateDefaultValues(args, recursionInfo);

                methodInfo.Invoke(genericObj, argValues);
            }
        }

        public static void SetGenericCollection(ICollection collection, Dictionary<Type, int> recursionInfo)
        {
            if (collection == null)
                return;

            var realisedListType = collection.GetType();
            var genericType = GetGenericCollectionType(realisedListType);
            var args = genericType.GenericTypeArguments();
            if (args.Length != 1)
            {
                Log(string.Format("Found a generic list that does not take one generic argument: {0}", realisedListType),
                    new Dictionary<string, string>() { { "ErrorCode", "FXD300039" } });

                return;
            }

            try
            {
                var addMethodInfo = realisedListType.GetMethodInfo("Add");
                var removeAllMethodInfo = realisedListType.GetMethodInfo("RemoveAll");
                if (addMethodInfo != null)
                {
                    if (removeAllMethodInfo != null && !args[0].IsValueType)
                    {
                        var parameterExpression = Expression.Parameter(args[0]);
                        var equalExpression = Expression.Equal(parameterExpression, Expression.Constant(null));
                        var delegateType = typeof(Predicate<>).MakeGenericType(args[0]);
                        var predicate = Expression.Lambda(delegateType, equalExpression, parameterExpression).Compile();
                        removeAllMethodInfo.Invoke(collection, new object[] { predicate });
                    }

                    if (collection.Count == 0)
                    {
                        var newElement = CreateDefaultValue(args[0], recursionInfo);
                        addMethodInfo.Invoke(collection, new object[] { newElement });
                        return;
                    }

                    foreach (var element in collection)
                    {
                        PopulateObjectInternal(element, recursionInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error occurred in SetGenericCollection, Collection Type: {0}, Element Type: {1}", realisedListType, args[0]);
                Log(message, ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300039" } });
            }
        }

        public static Array PopulateArray(Array array, Type elementType, Dictionary<Type, int> recursionInfo)
        {
            if (array.Length == 0)
                array = Array.CreateInstance(elementType, 1);

            var element0 = array.GetValue(0);
            if (element0 == null)
            {
                element0 = CreateDefaultValue(elementType, recursionInfo);
                array.SetValue(element0, 0);
            }
            else
            {
                element0 = PopulateObject(element0);
                array.SetValue(element0, 0);
            }

            return array;
        }

        public static Array PopulateArray(Type type, Dictionary<Type, int> recursionInfo)
        {
            var elementType = type.GetElementType();
            var array = Array.CreateInstance(elementType, 1);

            return PopulateArray(array, elementType, recursionInfo);
        }

        //TODO: replace with InAssignableFrom
        public static bool CanCast(Type toType, Type fromType)
        {
            if (toType.IsInterface())
            {
                var interfaceList = fromType.Interfaces().ToList();
                if (interfaceList.Contains(toType)) return true;
            }
            else
            {
                Type baseType = fromType;
                bool areSameTypes;
                do
                {
                    areSameTypes = baseType == toType;
                }
                while (!areSameTypes && (baseType = fromType.BaseType()) != null);

                if (areSameTypes) return true;
            }

            return false;
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, T>> GetPropertyAttributes<T>(Type fromType) where T : Attribute
        {
            var attributeType = typeof(T);
            var baseType = fromType;
            do
            {
                var propertyInfos = baseType.AllProperties();
                foreach (var propertyInfo in propertyInfos)
                {
                    var attributes = propertyInfo.GetCustomAttributes(attributeType, true);
                    foreach (T attribute in attributes)
                    {
                        yield return new KeyValuePair<PropertyInfo, T>(propertyInfo, attribute);
                    }
                }
            }
            while ((baseType = baseType.BaseType()) != null);
        }
    }
}

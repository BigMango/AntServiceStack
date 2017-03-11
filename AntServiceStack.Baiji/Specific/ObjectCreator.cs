using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using System.Linq;
using Freeway.Logging;
using System.Collections.Concurrent;

namespace AntServiceStack.Baiji.Specific
{
    public sealed class ObjectCreator
    {
        private static readonly ObjectCreator instance = new ObjectCreator();
        private static readonly ILog log = LogManager.GetLogger(typeof(ObjectCreator));

        public static ObjectCreator Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Static generic dictionary type used for creating new dictionary instances 
        /// </summary>
        private static readonly Type GenericMapType = typeof(Dictionary<,>);

        /// <summary>
        /// Static generic list type used for creating new array instances
        /// </summary>
        private static readonly Type GenericListType = typeof(List<>);

        /// <summary>
        /// Static generic nullable type used for creating new nullable instances
        /// </summary>
        private readonly Type GenericNullableType = typeof(Nullable<>);

        private readonly Assembly entryAssembly;
        private readonly bool diffAssembly;

        public delegate object CtorDelegate();

        private readonly Type ctorType = typeof(CtorDelegate);
        private readonly Dictionary<NameCtorKey, CtorDelegate> ctors = new Dictionary<NameCtorKey,CtorDelegate>();
        private readonly ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
        
        private ObjectCreator()
        {
#if !(SILVERLIGHT || WINDOWS_PHONE)
            var execAssembly = Assembly.GetExecutingAssembly();
            entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null && execAssembly != entryAssembly)
            // entryAssembly returns null when running from NUnit
            {
                diffAssembly = true;
            }

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblies.TryAdd(assembly.FullName, assembly);
#endif
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            assemblies.TryAdd(args.LoadedAssembly.FullName, args.LoadedAssembly);
        }

        public struct NameCtorKey : IEquatable<NameCtorKey>
        {
            public string Name
            {
                get;
                private set;
            }

            public SchemaType Type
            {
                get;
                private set;
            }

            public NameCtorKey(string name, SchemaType type)
                : this()
            {
                Name = name;
                Type = type;
            }

            public bool Equals(NameCtorKey other)
            {
                return Equals(other.Name, Name) && other.Type == Type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (obj.GetType() != typeof(NameCtorKey))
                {
                    return false;
                }
                return Equals((NameCtorKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Type.GetHashCode();
                }
            }

            public static bool operator ==(NameCtorKey left, NameCtorKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(NameCtorKey left, NameCtorKey right)
            {
                return !left.Equals(right);
            }
        }

        /// <summary>
        /// Find the type with the given name
        /// </summary>
        /// <param name="name">the object type to locate</param>
        /// <param name="throwError">whether or not to throw an error if the type wasn't found</param>
        /// <returns>the object type, or <c>null</c> if not found</returns>
        private Type FindType(string name, bool throwError)
        {
            Type type;
            if (diffAssembly)
            {
                // entry assembly different from current assembly, try entry assembly first
                type = entryAssembly.GetType(name);
                if (type == null) // now try current assembly and mscorlib
                {
                    type = Type.GetType(name);
                }
            }
            else
            {
                type = Type.GetType(name);
            }

            if (type == null) // type is still not found, need to loop through all loaded assemblies
            {
                string[] fullNames = assemblies.Keys.ToArray();
                foreach (var fullName in fullNames)
                {
                    Assembly assembly;
                    if (!assemblies.TryGetValue(fullName, out assembly) || assembly == null)
                        continue;

                    // Fix for Mono 3.0.10
                    if (assembly.FullName.StartsWith("MonoDevelop.NUnit"))
                    {
                        continue;
                    }

                    Type[] types = null;

                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch
                    {
                        log.Info("Unable to get type of assembly: " + fullName, new Dictionary<string, string> { { "ErrorCode", "FXD301022" } });
                        assemblies.TryRemove(fullName, out assembly);
                        continue;
                    }

                    // Change the search to look for Types by both NAME and FULLNAME
                    foreach (Type t in types)
                    {
                        if (name == t.Name || name == t.FullName)
                        {
                            type = t;
                        }
                    }

                    if (type != null)
                    {
                        break;
                    }
                }
            }

            if (null == type && throwError)
            {
                throw new BaijiException("Unable to find type " + name + " in all loaded assemblies");
            }

            return type;
        }

        /// <summary>
        /// Gets the type for the specified schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Type GetType(Schema.Schema schema)
        {
            switch (schema.Type)
            {
                case SchemaType.Null:
                    break;
                case SchemaType.Boolean:
                    return typeof(bool);
                case SchemaType.Int:
                    return typeof(int);
                case SchemaType.Long:
                    return typeof(long);
                case SchemaType.Float:
                    return typeof(float);
                case SchemaType.Double:
                    return typeof(double);
                case SchemaType.Bytes:
                    return typeof(byte[]);
                case SchemaType.String:
                    return typeof(string);
                case SchemaType.DateTime:
                    return typeof(DateTime);
                case SchemaType.Array:
                {
                    ArraySchema arrSchema = schema as ArraySchema;
                    Type itemSchema = GetType(arrSchema.ItemSchema);

                    return GenericListType.MakeGenericType(new[] {itemSchema});
                }
                case SchemaType.Map:
                {
                    MapSchema mapSchema = schema as MapSchema;
                    Type itemSchema = GetType(mapSchema.ValueSchema);

                    return GenericMapType.MakeGenericType(new[] {typeof(string), itemSchema});
                }
                case SchemaType.Enumeration:
                case SchemaType.Record:
                {
                    // Should all be named types
                    var named = schema as NamedSchema;
                    if (null != named)
                    {
                        return FindType(named.Fullname, true);
                    }
                    break;
                }
                case SchemaType.Union:
                {
                    UnionSchema unSchema = schema as UnionSchema;
                    if (null != unSchema && unSchema.Count == 2)
                    {
                        var s1 = unSchema.Schemas[0];
                        var s2 = unSchema.Schemas[1];

                        // Nullable ?
                        Type itemType = null;
                        if (s1.Type == SchemaType.Null)
                        {
                            itemType = GetType(s2);
                        }
                        else if (s2.Type == SchemaType.Null)
                        {
                            itemType = GetType(s1);
                        }

                        if (null != itemType)
                        {
                            if (itemType.IsValueType && !itemType.IsEnum)
                            {
                                try
                                {
                                    return GenericNullableType.MakeGenericType(new[] {itemType});
                                }
                                catch
                                {
                                }
                            }
                            return itemType;
                        }
                    }

                    return typeof(object);
                }
            }

            // Fallback
            return FindType(schema.Name, true);
        }

        /// <summary>
        /// Gets the type of the specified type name
        /// </summary>
        /// <param name="name">name of the object to get type of</param>
        /// <param name="schemaType">schema type for the object</param>
        /// <returns>Type</returns>
        public Type GetType(string name, SchemaType schemaType)
        {
            Type type = FindType(name, true);

            if (schemaType == SchemaType.Map)
            {
                type = GenericMapType.MakeGenericType(new[] {typeof(string), type});
            }
            else if (schemaType == SchemaType.Array)
            {
                type = GenericListType.MakeGenericType(new[] {type});
            }

            return type;
        }

        /// <summary>
        /// Gets the default constructor for the specified type
        /// </summary>
        /// <param name="name">name of object for the type</param>
        /// <param name="schemaType">schema type for the object</param>
        /// <param name="type">type of the object</param>
        /// <returns>Default constructor for the type</returns>
        public CtorDelegate GetConstructor(string name, SchemaType schemaType, Type type)
        {
            ConstructorInfo ctorInfo = type.GetConstructor(Type.EmptyTypes);
            if (ctorInfo == null)
            {
                throw new BaijiException("Class " + name + " has no default constructor");
            }
#if !(SILVERLIGHT || WINDOWS_PHONE)
            DynamicMethod dynMethod = new DynamicMethod("DM$OBJ_FACTORY_" + name, typeof(object), null, type, true);
            ILGenerator ilGen = dynMethod.GetILGenerator();
            ilGen.Emit(OpCodes.Nop);
            ilGen.Emit(OpCodes.Newobj, ctorInfo);
            ilGen.Emit(OpCodes.Ret);

            return (CtorDelegate)dynMethod.CreateDelegate(ctorType);
#else
            return (CtorDelegate)(() => ctorInfo.Invoke(null));
#endif
        }

        /// <summary>
        /// Creates new instance of the given type
        /// </summary>
        /// <param name="name">fully qualified name of the type</param>
        /// <param name="schemaType">type of schema</param>
        /// <returns>new object of the given type</returns>
        public object New(string name, SchemaType schemaType)
        {
            NameCtorKey key = new NameCtorKey(name, schemaType);

            CtorDelegate ctor;
            if (!ctors.TryGetValue(key, out ctor))
            {
                lock (ctors)
                {
                    if (!ctors.TryGetValue(key, out ctor))
                    {
                        Type type = GetType(name, schemaType);
                        ctor = GetConstructor(name, schemaType, type);
                        ctors.Add(key, ctor);
                    }
                }
            }
            return ctor();
        }
    }
}
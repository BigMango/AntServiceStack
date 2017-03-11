using System;
using System.Collections.Generic;
using System.Text;

namespace AntServiceStack.Baiji.Schema
{
    internal sealed class TypeHelper
    {
        private const string Object = "System.Object";
        private const char At = '@';
        private const char Dot = '.';

        internal static string GetType(Schema schema, bool nullable, ref bool nullableEnum)
        {
            switch (schema.Type)
            {
                case SchemaType.Null:
                    return "System.Object";

                case SchemaType.Boolean:
                    if (nullable)
                    {
                        return "System.Nullable<bool>";
                    }
                    else
                    {
                        return typeof(bool).ToString();
                    }
                case SchemaType.Int:
                    if (nullable)
                    {
                        return "System.Nullable<int>";
                    }
                    else
                    {
                        return typeof(int).ToString();
                    }
                case SchemaType.Long:
                    if (nullable)
                    {
                        return "System.Nullable<long>";
                    }
                    else
                    {
                        return typeof(long).ToString();
                    }
                case SchemaType.Float:
                    if (nullable)
                    {
                        return "System.Nullable<float>";
                    }
                    else
                    {
                        return typeof(float).ToString();
                    }
                case SchemaType.Double:
                    if (nullable)
                    {
                        return "System.Nullable<double>";
                    }
                    else
                    {
                        return typeof(double).ToString();
                    }

                case SchemaType.Bytes:
                    return typeof(byte[]).ToString();

                case SchemaType.String:
                    return typeof(string).ToString();

                case SchemaType.DateTime:
                    return typeof(DateTime).ToString();

                case SchemaType.Enumeration:
                    var namedSchema = schema as NamedSchema;
                    if (null == namedSchema)
                    {
                        throw new SchemaParseException("Unable to cast schema into a named schema");
                    }
                    if (nullable)
                    {
                        nullableEnum = true;
                        return "System.Nullable<" + TypeHelper.Mangle(namedSchema.Fullname) + ">";
                    }
                    else
                    {
                        return Mangle(namedSchema.Fullname);
                    }

                case SchemaType.Record:
                    namedSchema = schema as NamedSchema;
                    if (null == namedSchema)
                    {
                        throw new SchemaParseException("Unable to cast schema into a named schema");
                    }
                    return Mangle(namedSchema.Fullname);

                case SchemaType.Array:
                    var arraySchema = schema as ArraySchema;
                    if (null == arraySchema)
                    {
                        throw new SchemaParseException("Unable to cast schema into an array schema");
                    }

                    return "List<" + GetType(arraySchema.ItemSchema, false, ref nullableEnum) + ">";

                case SchemaType.Map:
                    var mapSchema = schema as MapSchema;
                    if (null == mapSchema)
                    {
                        throw new SchemaParseException("Unable to cast schema into a map schema");
                    }
                    return "Dictionary<string, " + GetType(mapSchema.ValueSchema, false, ref nullableEnum) + ">";

                case SchemaType.Union:
                    var unionSchema = schema as UnionSchema;
                    if (null == unionSchema)
                    {
                        throw new SchemaParseException("Unable to cast schema into a union schema");
                    }
                    var nullableType = GetNullableType(unionSchema);
                    if (null == nullableType)
                    {
                        return Object;
                    }
                    else
                    {
                        return GetType(nullableType, true, ref nullableEnum);
                    }
            }
            throw new SchemaParseException("Unable to generate CodeTypeReference for " + schema.Name + " type " + schema.Type);
        }

        private static Schema GetNullableType(UnionSchema schema)
        {
            Schema ret = null;
            if (schema.Count == 2)
            {
                bool nullable = false;
                foreach (var childSchema in schema.Schemas)
                {
                    if (childSchema.Type == SchemaType.Null)
                    {
                        nullable = true;
                    }
                    else
                    {
                        ret = childSchema;
                    }
                }
                if (!nullable)
                {
                    ret = null;
                }
            }
            return ret;
        }

        private static string Mangle(string name)
        {
            var builder = new StringBuilder();
            string[] names = name.Split(Dot);
            for (int i = 0; i < names.Length; ++i)
            {
                if (ReservedKeywords.Contains(names[i]))
                {
                    builder.Append(At);
                }
                builder.Append(names[i]);
                builder.Append(Dot);
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        private static readonly List<string> ReservedKeywords = new List<string>()
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
            "value",
            "partial"
        };
    }
}
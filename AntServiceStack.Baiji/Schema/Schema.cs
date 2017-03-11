using System;
using System.IO;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Base class for all schema types
    /// </summary>
    public abstract class Schema
    {
        /// <summary>
        /// Schema type property
        /// </summary>
        public SchemaType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Additional JSON attributes apart from those defined in the Baiji spec
        /// </summary>
        internal PropertyMap Props
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor for schema class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="props"></param>
        protected Schema(SchemaType type, PropertyMap props)
        {
            Type = type;
            Props = props;
        }

        /// <summary>
        /// The name of this schema. If this is a named schema such as an enum, it returns the fully qualified
        /// name for the schema. For other schemas, it returns the type of the schema.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Static method to return new instance of schema object
        /// </summary>
        /// <param name="jtok">JSON object</param>
        /// <param name="names">list of named schemas already read</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        /// <returns>new Schema object</returns>
        internal static Schema ParseJson(JToken jtok, SchemaNames names, string encspace)
        {
            if (null == jtok)
            {
                throw new ArgumentNullException("jtok", "jtok cannot be null.");
            }

            if (jtok.Type == JTokenType.String)
                // primitive schema with no 'type' property or primitive or named type of a record field
            {
                string value = (string)jtok;

                PrimitiveSchema ps = PrimitiveSchema.NewInstance(value);
                if (null != ps)
                {
                    return ps;
                }

                NamedSchema schema = null;
                if (names.TryGetValue(value, null, encspace, out schema))
                {
                    return schema;
                }

                throw new SchemaParseException("Undefined name: " + value);
            }

            if (jtok is JArray) // union schema with no 'type' property or union type for a record field
            {
                return UnionSchema.NewInstance(jtok as JArray, null, names, encspace);
            }

            if (jtok is JObject) // JSON object with open/close parenthesis, it must have a 'type' property
            {
                JObject jo = jtok as JObject;

                JToken jtype = jo["type"];
                if (null == jtype)
                {
                    throw new SchemaParseException("Property type is required");
                }

                var props = JsonHelper.GetProperties(jtok);

                if (jtype.Type == JTokenType.String)
                {
                    string type = (string)jtype;

                    if (type.Equals("array"))
                    {
                        return ArraySchema.NewInstance(jtok, props, names, encspace);
                    }
                    if (type.Equals("map"))
                    {
                        return MapSchema.NewInstance(jtok, props, names, encspace);
                    }

                    Schema schema = PrimitiveSchema.NewInstance((string)type, props);
                    if (null != schema)
                    {
                        return schema;
                    }

                    return NamedSchema.NewInstance(jo, props, names, encspace);
                }
                else if (jtype.Type == JTokenType.Array)
                {
                    return UnionSchema.NewInstance(jtype as JArray, props, names, encspace);
                }
            }
            throw new BaijiTypeException("Invalid JSON for schema: " + jtok);
        }

        /// <summary>
        /// Parses a given JSON string to create a new schema object
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>new Schema object</returns>
        public static Schema Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json", "json cannot be null.");
            }
            return Parse(json.Trim(), new SchemaNames(), null); // standalone schema, so no enclosing namespace
        }

        /// <summary>
        /// Parses a JSON string to create a new schema object
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <param name="names">list of named schemas already read</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        /// <returns>new Schema object</returns>
        internal static Schema Parse(string json, SchemaNames names, string encspace)
        {
            Schema sc = PrimitiveSchema.NewInstance(json);
            if (null != sc)
            {
                return sc;
            }

            try
            {
                bool isArray = json.StartsWith("[") && json.EndsWith("]");
                JContainer j = isArray ? (JContainer)JArray.Parse(json) : (JContainer)JObject.Parse(json);
                return ParseJson(j, names, encspace);
            }
            catch (JsonSerializationException ex)
            {
                throw new SchemaParseException("Could not parse. " + ex.Message + Environment.NewLine + json);
            }
        }

        /// <summary>
        /// Returns the canonical JSON representation of this schema.
        /// </summary>
        /// <returns>The canonical JSON representation of this schema.</returns>
        public override string ToString()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    if (this is PrimitiveSchema || this is UnionSchema)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName("type");
                    }

                    WriteJson(jsonWriter, new SchemaNames(), null); // stand alone schema, so no enclosing name space

                    if (this is PrimitiveSchema || this is UnionSchema)
                    {
                        jsonWriter.WriteEndObject();
                    }

                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Writes opening { and 'type' property 
        /// </summary>
        /// <param name="writer">JSON writer</param>
        private void WriteStartObject(JsonTextWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(GetTypeString(Type));
        }

        /// <summary>
        /// Returns symbol name for the given schema type
        /// </summary>
        /// <param name="type">schema type</param>
        /// <returns>symbol name</returns>
        private static string GetTypeString(SchemaType type)
        {
            return type != SchemaType.Enumeration ? type.ToString().ToLower() : "enum";
        }

        /// <summary>
        /// Default implementation for writing schema properties in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        protected internal virtual void WriteJsonFields(JsonTextWriter writer, SchemaNames names, string encspace)
        {
        }

        /// <summary>
        /// Writes schema object in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        protected internal virtual void WriteJson(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            WriteStartObject(writer);
            WriteJsonFields(writer, names, encspace);
            if (null != Props)
            {
                Props.WriteJson(writer);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Returns the schema's custom property value given the property name
        /// </summary>
        /// <param name="key">custom property name</param>
        /// <returns>custom property value</returns>
        public string GetProperty(string key)
        {
            if (null == Props)
            {
                return null;
            }
            string v;
            return (Props.TryGetValue(key, out v)) ? v : null;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var that = obj as Schema;
            if (that == null)
            {
                return false;
            }
            if (Type != that.Type)
            {
                return false;
            }
            return ObjectUtils.AreEqual(Props, that.Props);
        }

        /// <summary>
        /// Hash code function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() + ObjectUtils.GetHashCode(Props);
        }
    }
}
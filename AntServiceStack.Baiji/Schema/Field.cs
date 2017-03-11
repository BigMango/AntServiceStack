using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for fields defined in a record
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Enum for the sorting order of record fields
        /// </summary>
        public enum SortOrder
        {
            Ascending,
            Descending,
            Ignore
        }

        /// <summary>
        /// Name of the field.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// List of aliases for the field name
        /// </summary>
        public readonly IList<string> Aliases;

        /// <summary>
        /// Position of the field within its record.
        /// </summary>
        public int Pos
        {
            get;
            private set;
        }

        /// <summary>
        /// Documentation for the field, if any. Null if there is no documentation.
        /// </summary>
        public string Documentation
        {
            get;
            private set;
        }

        /// <summary>
        /// The default value for the field stored as JSON object, if defined. Otherwise, null.
        /// </summary>
        public JToken DefaultValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Order of the field
        /// </summary>
        public SortOrder? Ordering
        {
            get;
            private set;
        }

        /// <summary>
        /// Field type's schema
        /// </summary>
        public Schema Schema
        {
            get;
            private set;
        }

        /// <summary>
        /// Custom properties for the field. We don't store the fields custom properties in
        /// the field type's schema because if the field type is only a reference to the schema 
        /// instead of an actual schema definition, then the schema could already have it's own set 
        /// of custom properties when it was previously defined.
        /// </summary>
        private readonly PropertyMap Props;

        /// <summary>
        /// Static comparer object for JSON objects such as the fields default value
        /// </summary>
        private static readonly JTokenEqualityComparer JTokenComparer = new JTokenEqualityComparer();

        /// <summary>
        /// Constructor for the field class
        /// </summary>
        /// <param name="schema">schema for the field type</param>
        /// <param name="name">name of the field</param>
        /// <param name="aliases">list of aliases for the name of the field</param>
        /// <param name="pos">position of the field</param>
        /// <param name="doc">documentation for the field</param>
        /// <param name="defaultValue">field's default value if it exists</param>
        /// <param name="sortorder">sort order of the field</param>
        /// <param name="props"></param>
        public Field(Schema schema, string name, IList<string> aliases, int pos, string doc,
            JToken defaultValue, SortOrder sortorder, PropertyMap props)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "name cannot be null.");
            }
            if (schema == null)
            {
                throw new ArgumentNullException("schema", "schema cannot be null.");
            }
            Schema = schema;
            Name = name;
            Aliases = aliases;
            Pos = pos;
            Documentation = doc;
            DefaultValue = defaultValue;
            Ordering = sortorder;
            Props = props;
        }

        /// <summary>
        /// Writes the Field class in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace for the field</param>
        protected internal void WriteJson(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            writer.WriteStartObject();
            JsonHelper.WriteIfNotNullOrEmpty(writer, "name", Name);
            JsonHelper.WriteIfNotNullOrEmpty(writer, "doc", Documentation);

            if (null != DefaultValue)
            {
                writer.WritePropertyName("default");
                DefaultValue.WriteTo(writer, null);
            }
            if (null != Schema)
            {
                writer.WritePropertyName("type");
                Schema.WriteJson(writer, names, encspace);
            }

            if (null != Props)
            {
                Props.WriteJson(writer);
            }

            if (null != Aliases)
            {
                writer.WritePropertyName("aliases");
                writer.WriteStartArray();
                foreach (string name in Aliases)
                {
                    writer.WriteValue(name);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Parses the 'aliases' property from the given JSON token
        /// </summary>
        /// <param name="jtok">JSON object to read</param>
        /// <returns>List of string that represents the list of alias. If no 'aliases' specified, then it returns null.</returns>
        internal static IList<string> GetAliases(JToken jtok)
        {
            JToken jaliases = jtok["aliases"];
            if (null == jaliases)
            {
                return null;
            }

            if (jaliases.Type != JTokenType.Array)
            {
                throw new SchemaParseException("Aliases must be of format JSON array of strings");
            }

            var aliases = new List<string>();
            foreach (JToken jalias in jaliases)
            {
                if (jalias.Type != JTokenType.String)
                {
                    throw new SchemaParseException("Aliases must be of format JSON array of strings");
                }

                aliases.Add((string)jalias);
            }
            return aliases;
        }

        /// <summary>
        /// Returns the field's custom property value given the property name
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
        /// Compares two field objects
        /// </summary>
        /// <param name="obj">field to compare with this field</param>
        /// <returns>true if two fields are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            var that = obj as Field;
            if (that == null)
            {
                return false;
            }
            return ObjectUtils.AreEqual(that.Name, Name) && that.Pos == Pos &&
                   ObjectUtils.AreEqual(that.Documentation, Documentation)
                   && ObjectUtils.AreEqual(that.Ordering, Ordering) &&
                   JTokenComparer.Equals(that.DefaultValue, DefaultValue)
                   && that.Schema.Equals(Schema) && ObjectUtils.AreEqual(that.Props, Props);
        }

        /// <summary>
        /// Hash code function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 17 * Name.GetHashCode() + Pos + 19 * ObjectUtils.GetHashCode(Documentation) +
                   23 * ObjectUtils.GetHashCode(Ordering) + 29 * ObjectUtils.GetHashCode(DefaultValue) +
                   31 * Schema.GetHashCode() + 37 * ObjectUtils.GetHashCode(Props);
        }
    }
}
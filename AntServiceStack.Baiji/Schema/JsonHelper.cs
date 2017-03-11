using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    internal static class JsonHelper
    {
        /// <summary>
        /// Static function to parse custom properties (not defined in the Baiji spec) from the given JSON object
        /// </summary>
        /// <param name="token">JSON object to parse</param>
        /// <returns>Property map if custom properties were found, null if no custom properties found</returns>
        public static PropertyMap GetProperties(JToken token)
        {
            var props = new PropertyMap();
            props.Parse(token);
            return props.Count > 0 ? props : null;
        }

        /// <summary>
        /// Retrieves the optional string property value for the given property name from the given JSON object.
        /// This throws an exception if property exists but it is not a string.
        /// </summary>
        /// <param name="jtok">JSON object to read</param>
        /// <param name="field">property name</param>
        /// <returns>property value if property exists, null if property doesn't exist in the JSON object</returns>
        public static string GetOptionalString(JToken jtok, string field)
        {
            if (null == jtok)
            {
                throw new ArgumentNullException("jtok", "jtok cannot be null.");
            }
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field", "field cannot be null.");
            }

            JToken child = jtok[field];
            if (child == null)
            {
                return null;
            }

            if (child.Type == JTokenType.Null)
            {
                return null;
            }

            if (child.Type == JTokenType.String)
            {
                string value = child.ToString();
                return value.Trim('\"');
            }
            throw new SchemaParseException("Field " + field + " is not a string");
        }

        /// <summary>
        /// Retrieves the required string property value for the given property name from the given JSON object.
        /// </summary>
        /// <param name="jtok">JSON object to read</param>
        /// <param name="field">property name</param>
        /// <returns>property value</returns>
        public static string GetRequiredString(JToken jtok, string field)
        {
            string value = GetOptionalString(jtok, field);
            if (string.IsNullOrEmpty(value))
            {
                throw new SchemaParseException(string.Format("No \"{0}\" JSON field: {1}", field, jtok));
            }
            return value;
        }

        /// <summary>
        /// Retrieves the required int property value for the given property name from the given JSON object.
        /// </summary>
        /// <param name="jtok">JSON object to read</param>
        /// <param name="field">property name</param>
        /// <returns>property value</returns>
        public static int GetRequiredInteger(JToken jtok, string field)
        {
            EnsureValidFieldName(field);
            JToken child = jtok[field];
            if (null == child)
            {
                throw new SchemaParseException(string.Format("No \"{0}\" JSON field: {1}", field, jtok));
            }

            if (child.Type == JTokenType.Integer)
            {
                return (int)child;
            }
            throw new SchemaParseException("Field " + field + " is not an integer");
        }

        /// <summary>
        /// Retrieves the optional boolean property value for the given property name from the given JSON object.
        /// </summary>
        /// <param name="jtok">JSON object to read</param>
        /// <param name="field">property name</param>
        /// <returns>null if property doesn't exist, otherise returns property boolean value</returns>
        public static bool? GetOptionalBoolean(JToken jtok, string field)
        {
            if (null == jtok)
            {
                throw new ArgumentNullException("jtok", "jtok cannot be null.");
            }
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field", "field cannot be null.");
            }

            JToken child = jtok[field];
            if (null == child)
            {
                return null;
            }

            if (child.Type == JTokenType.Boolean)
            {
                return (bool)child;
            }

            throw new SchemaParseException("Field " + field + " is not a boolean");
        }

        /// <summary>
        /// Writes JSON property name and value if value is not null
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="key">property name</param>
        /// <param name="value">property value</param>
        internal static void WriteIfNotNullOrEmpty(JsonTextWriter writer, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            writer.WritePropertyName(key);
            writer.WriteValue(value);
        }

        /// <summary>
        /// Checks if given name is not null or empty
        /// </summary>
        /// <param name="name"></param>
        private static void EnsureValidFieldName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
        }
    }
}
using System;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for map schemas
    /// </summary>
    public class MapSchema : UnnamedSchema
    {
        /// <summary>
        /// Schema for map values type
        /// </summary>
        public Schema ValueSchema
        {
            get;
            set;
        }

        /// <summary>
        /// Static function to return new instance of map schema
        /// </summary>
        /// <param name="jtok">JSON object for the map schema</param>
        /// <param name="names">list of named schemas already read</param>
        /// <param name="encspace">enclosing namespace of the map schema</param>
        /// <returns></returns>
        internal static MapSchema NewInstance(JToken jtok, PropertyMap props, SchemaNames names, string encspace)
        {
            JToken jvalue = jtok["values"];
            if (null == jvalue)
            {
                throw new BaijiTypeException("Map does not have 'values'");
            }

            return new MapSchema(ParseJson(jvalue, names, encspace), props);
        }

        /// <summary>
        /// Constructor for map schema class
        /// </summary>
        /// <param name="valueSchema">schema for map values type</param>
        /// <param name="props"></param>
        public MapSchema(Schema valueSchema, PropertyMap props)
            : base(SchemaType.Map, props)
        {
            if (null == valueSchema)
            {
                throw new ArgumentNullException("valueSchema", "valueSchema cannot be null.");
            }
            ValueSchema = valueSchema;
        }

        /// <summary>
        /// Writes map schema in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the map schema</param>
        protected internal override void WriteJsonFields(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            writer.WritePropertyName("values");
            ValueSchema.WriteJson(writer, names, encspace);
        }

        /// <summary>
        /// Compares equality of two map schemas
        /// </summary>
        /// <param name="obj">map schema to compare against this schema</param>
        /// <returns>true if two schemas are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            var that = obj as MapSchema;
            if (that == null)
            {
                return false;
            }
            return ValueSchema.Equals(that.ValueSchema) && ObjectUtils.AreEqual(that.Props, Props);
        }

        /// <summary>
        /// Hashcode function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 29 * ValueSchema.GetHashCode() + ObjectUtils.GetHashCode(Props);
        }
    }
}
using System;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for array type schemas
    /// </summary>
    public class ArraySchema : UnnamedSchema
    {
        /// <summary>
        /// Schema for the array 'type' attribute
        /// </summary>
        public Schema ItemSchema
        {
            get;
            set;
        }

        /// <summary>
        /// Static class to return a new instance of ArraySchema
        /// </summary>
        /// <param name="token">JSON object for the array schema</param>
        /// <param name="props"></param>
        /// <param name="names">list of named schemas already parsed</param>
        /// <param name="encspace">enclosing namespace for the array schema</param>
        /// <returns></returns>
        internal static ArraySchema NewInstance(JToken token, PropertyMap props, SchemaNames names, string encspace)
        {
            JToken jitem = token["items"];
            if (jitem == null)
            {
                throw new BaijiTypeException("Array does not have 'items'");
            }
            return new ArraySchema(ParseJson(jitem, names, encspace), props);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">schema for the array items type</param>
        /// <param name="props"></param>
        public ArraySchema(Schema items, PropertyMap props)
            : base(SchemaType.Array, props)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }
            ItemSchema = items;
        }

        /// <summary>
        /// Writes the array schema in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace</param>
        protected internal override void WriteJsonFields(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            writer.WritePropertyName("items");
            ItemSchema.WriteJson(writer, names, encspace);
        }

        /// <summary>
        /// Function to compare equality of two array schemas
        /// </summary>
        /// <param name="obj">other array schema</param>
        /// <returns>true two schemas are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is ArraySchema)
            {
                var that = obj as ArraySchema;
                if (ItemSchema.Equals(that.ItemSchema))
                {
                    return ObjectUtils.AreEqual(that.Props, Props);
                }
            }
            return false;
        }

        /// <summary>
        /// Hashcode function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 29 * ItemSchema.GetHashCode() + ObjectUtils.GetHashCode(Props);
        }
    }
}
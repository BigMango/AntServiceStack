using System.Collections.Generic;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for schemas of primitive types
    /// </summary>
    public sealed class PrimitiveSchema : UnnamedSchema
    {
        private static readonly IDictionary<string, SchemaType> _types = new Dictionary<string, SchemaType>();

        static PrimitiveSchema()
        {
            _types.Add("null", SchemaType.Null);
            _types.Add("boolean", SchemaType.Boolean);
            _types.Add("int", SchemaType.Int);
            _types.Add("long", SchemaType.Long);
            _types.Add("float", SchemaType.Float);
            _types.Add("double", SchemaType.Double);
            _types.Add("bytes", SchemaType.Bytes);
            _types.Add("string", SchemaType.String);
            _types.Add("datetime", SchemaType.DateTime);
            _types.Add("decimal", SchemaType.Decimal);
        }

        /// <summary>
        /// Constructor for primitive schema
        /// </summary>
        /// <param name="type"></param>
        /// <param name="props"></param>
        public PrimitiveSchema(SchemaType type, PropertyMap props)
            : base(type, props)
        {
        }

        /// <summary>
        /// Static function to return new instance of primitive schema
        /// </summary>
        /// <param name="type">primitive type</param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static PrimitiveSchema NewInstance(string type, PropertyMap props = null)
        {
            const string q = "\"";
            if (type.StartsWith(q) && type.EndsWith(q))
            {
                type = type.Substring(1, type.Length - 2);
            }
            SchemaType schemaType;
            if (_types.TryGetValue(type, out schemaType))
            {
                return new PrimitiveSchema(schemaType, props);
            }
            return null;
        }

        /// <summary>
        /// Writes primitive schema in JSON format
        /// </summary>
        /// <param name="w"></param>
        /// <param name="names"></param>
        /// <param name="encspace"></param>
        protected internal override void WriteJson(JsonTextWriter w, SchemaNames names, string encspace)
        {
            w.WriteValue(Name);
        }

        /// <summary>
        /// Function to compare equality of two primitive schemas
        /// </summary>
        /// <param name="obj">other primitive schema</param>
        /// <returns>true two schemas are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            var that = obj as PrimitiveSchema;
            if (that == null)
            {
                return false;
            }
            return Type == that.Type && ObjectUtils.AreEqual(that.Props, Props);
        }

        /// <summary>
        /// Hashcode function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 13 * Type.GetHashCode() + ObjectUtils.GetHashCode(Props);
        }
    }
}
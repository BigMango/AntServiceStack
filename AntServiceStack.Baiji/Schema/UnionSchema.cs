using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for union schemas
    /// </summary>
    public class UnionSchema : UnnamedSchema
    {
        /// <summary>
        /// List of schemas in the union
        /// </summary>
        public IList<Schema> Schemas
        {
            get;
            private set;
        }

        /// <summary>
        /// Count of schemas in the union
        /// </summary>
        public int Count
        {
            get
            {
                return Schemas.Count;
            }
        }

        /// <summary>
        /// Static function to return instance of the union schema
        /// </summary>
        /// <param name="jarr">JSON object for the union schema</param>
        /// <param name="names">list of named schemas already read</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        /// <returns>new UnionSchema object</returns>
        internal static UnionSchema NewInstance(JArray jarr, PropertyMap props, SchemaNames names, string encspace)
        {
            List<Schema> schemas = new List<Schema>();
            IDictionary<string, string> uniqueSchemas = new Dictionary<string, string>();

            foreach (JToken jvalue in jarr)
            {
                Schema unionType = ParseJson(jvalue, names, encspace);
                if (null == unionType)
                {
                    throw new SchemaParseException("Invalid JSON in union" + jvalue);
                }

                string name = unionType.Name;
                if (uniqueSchemas.ContainsKey(name))
                {
                    throw new SchemaParseException("Duplicate type in union: " + name);
                }

                uniqueSchemas.Add(name, name);
                schemas.Add(unionType);
            }

            return new UnionSchema(schemas, props);
        }

        /// <summary>
        /// Contructor for union schema
        /// </summary>
        /// <param name="schemas"></param>
        public UnionSchema(IList<Schema> schemas, PropertyMap props) : base(SchemaType.Union, props)
        {
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            Schemas = schemas;
        }

        /// <summary>
        /// Returns the schema at the given branch.
        /// </summary>
        /// <param name="index">Index to the branch, starting with 0.</param>
        /// <returns>The branch corresponding to the given index.</returns>
        public Schema this[int index]
        {
            get
            {
                return Schemas[index];
            }
        }

        /// <summary>
        /// Writes union schema in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        protected internal override void WriteJson(JsonTextWriter writer, SchemaNames names,
            string encspace)
        {
            writer.WriteStartArray();
            foreach (Schema schema in Schemas)
            {
                schema.WriteJson(writer, names, encspace);
            }
            writer.WriteEndArray();
        }

        /// <summary>
        /// Compares two union schema objects
        /// </summary>
        /// <param name="obj">union schema object to compare against this schema</param>
        /// <returns>true if objects are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is UnionSchema))
            {
                return false;
            }
            var that = obj as UnionSchema;
            if (that.Count == Count)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!that[i].Equals(this[i]))
                    {
                        return false;
                    }
                }
                return ObjectUtils.AreEqual(that.Props, Props);
            }
            return false;
        }

        /// <summary>
        /// Hash code function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int result = 53;
            foreach (Schema schema in Schemas)
            {
                result += 89 * schema.GetHashCode();
            }
            result += ObjectUtils.GetHashCode(Props);
            return result;
        }
    }
}
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for record schemas
    /// </summary>
    public class RecordSchema : NamedSchema
    {
        /// <summary>
        /// List of fields in the record
        /// </summary>
        public IList<Field> Fields
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of fields in the record
        /// </summary>
        public int Count
        {
            get
            {
                return Fields.Count;
            }
        }

        /// <summary>
        /// Map of field name and Field object for faster field lookups
        /// </summary>
        private readonly IDictionary<string, Field> _fieldLookup;

        private readonly IDictionary<string, Field> _fieldAliasLookup;
        private readonly bool _request;

        /// <summary>
        /// Static function to return new instance of the record schema
        /// </summary>
        /// <param name="jtok">JSON object for the record schema</param>
        /// <param name="props"></param>
        /// <param name="names">list of named schema already read</param>
        /// <param name="encspace">enclosing namespace of the records schema</param>
        /// <returns>new RecordSchema object</returns>
        internal static RecordSchema NewInstance(JToken jtok, PropertyMap props, SchemaNames names,
            string encspace)
        {
            bool request = false;
            JToken jfields = jtok["fields"]; // normal record
            if (null == jfields)
            {
                jfields = jtok["request"]; // anonymous record from messages
                if (null != jfields)
                {
                    request = true;
                }
            }
            if (null == jfields)
            {
                throw new SchemaParseException("'fields' cannot be null for record");
            }
            if (jfields.Type != JTokenType.Array)
            {
                throw new SchemaParseException("'fields' not an array for record");
            }

            var name = GetName(jtok, encspace);
            var aliases = GetAliases(jtok, name.Space, name.EncSpace);
            var doc = JsonHelper.GetOptionalString(jtok, "doc");
            var fields = new List<Field>();
            var fieldMap = new Dictionary<string, Field>();
            var fieldAliasMap = new Dictionary<string, Field>();
            var result = new RecordSchema(name, doc, aliases, props, fields, request, fieldMap, fieldAliasMap, names);

            int fieldPos = 0;
            foreach (JObject jfield in jfields)
            {
                string fieldName = JsonHelper.GetRequiredString(jfield, "name");
                Field field = CreateField(jfield, fieldPos++, names, name.Namespace);
                // add record namespace for field look up
                fields.Add(field);
                AddToFieldMap(fieldMap, fieldName, field);
                AddToFieldMap(fieldAliasMap, fieldName, field);

                if (null != field.Aliases)
                // add aliases to field lookup map so reader function will find it when writer field name appears only as an alias on the reader field
                {
                    foreach (string alias in field.Aliases)
                    {
                        AddToFieldMap(fieldAliasMap, alias, field);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Constructor for the record schema
        /// </summary>
        /// <param name="name">name of the record schema</param>
        /// <param name="doc"></param>
        /// <param name="aliases">list of aliases for the record name</param>
        /// <param name="props"></param>
        /// <param name="fields">list of fields for the record</param>
        /// <param name="request">true if this is an anonymous record with 'request' instead of 'fields'</param>
        public RecordSchema(SchemaName name, string doc, IList<SchemaName> aliases, PropertyMap props,
            IList<Field> fields, bool request)
            : base(SchemaType.Record, name, doc, aliases, props, new SchemaNames())
        {
            if (!request && null == name.Name)
            {
                throw new SchemaParseException("name cannot be null for record schema.");
            }
            Fields = fields ?? new List<Field>();
            this._request = request;
            var fieldMap = new Dictionary<string, Field>();
            var fieldAliasMap = new Dictionary<string, Field>();
            foreach (var field in Fields)
            {
                AddToFieldMap(fieldMap, field.Name, field);
                AddToFieldMap(fieldAliasMap, field.Name, field);
            }
            _fieldLookup = fieldMap;
            _fieldAliasLookup = fieldAliasMap;
        }

        /// <summary>
        /// Constructor for the record schema
        /// </summary>
        /// <param name="name">name of the record schema</param>
        /// <param name="doc"></param>
        /// <param name="aliases">list of aliases for the record name</param>
        /// <param name="props"></param>
        /// <param name="fields">list of fields for the record</param>
        /// <param name="request">true if this is an anonymous record with 'request' instead of 'fields'</param>
        /// <param name="fieldMap">map of field names and field objects</param>
        /// <param name="fieldAliasMap">map of field aliases and field objects</param>
        /// <param name="names">list of named schema already read</param>
        private RecordSchema(SchemaName name, string doc, IList<SchemaName> aliases, PropertyMap props,
            List<Field> fields, bool request, IDictionary<string, Field> fieldMap,
            IDictionary<string, Field> fieldAliasMap, SchemaNames names)
            : base(SchemaType.Record, name, doc, aliases, props, names)
        {
            if (!request && null == name.Name)
            {
                throw new SchemaParseException("name cannot be null for record schema.");
            }
            Fields = fields;
            this._request = request;
            _fieldLookup = fieldMap;
            _fieldAliasLookup = fieldAliasMap;
        }

        /// <summary>
        /// Creates a new field for the record
        /// </summary>
        /// <param name="jfield">JSON object for the field</param>
        /// <param name="pos">position number of the field</param>
        /// <param name="names">list of named schemas already read</param>
        /// <param name="encspace">enclosing namespace of the records schema</param>
        /// <returns>new Field object</returns>
        private static Field CreateField(JToken jfield, int pos, SchemaNames names, string encspace)
        {
            var name = JsonHelper.GetRequiredString(jfield, "name");
            var doc = JsonHelper.GetOptionalString(jfield, "doc");

            var jorder = JsonHelper.GetOptionalString(jfield, "order");
            Field.SortOrder sortorder = Field.SortOrder.Ignore;
            if (null != jorder)
            {
                sortorder = (Field.SortOrder)Enum.Parse(typeof(Field.SortOrder), jorder, true);
            }

            var aliases = Field.GetAliases(jfield);
            var props = JsonHelper.GetProperties(jfield);
            var defaultValue = jfield["default"];

            JToken jtype = jfield["type"];
            if (null == jtype)
            {
                throw new SchemaParseException("'type' was not found for field: " + name);
            }
            var schema = ParseJson(jtype, names, encspace);
            return new Field(schema, name, aliases, pos, doc, defaultValue, sortorder, props);
        }

        private static void AddToFieldMap(IDictionary<string, Field> map, string name, Field field)
        {
            if (map.ContainsKey(name.ToLower()))
            {
                throw new SchemaParseException("field or alias " + name + " is a duplicate name");
            }
            map.Add(name.ToLower(), field);
        }

        /// <summary>
        /// Returns the field with the given name.
        /// </summary>
        /// <param name="name">field name</param>
        /// <returns>Field object</returns>
        public Field this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException("name");
                }
                Field field;
                return (_fieldLookup.TryGetValue(name.ToLower(), out field)) ? field : null;
            }
        }

        public void AddField(Field field)
        {
            if (_fieldLookup.ContainsKey(field.Name.ToLower()))
            {
                throw new ArgumentException("Duplicate field: " + field.Name);
            }
            Fields.Add(field);
            AddToFieldMap(_fieldLookup, field.Name, field);
            AddToFieldMap(_fieldAliasLookup, field.Name, field);
        }

        /// <summary>
        /// Returns true if and only if the record contains a field by the given name.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>true if the field exists, false otherwise</returns>
        public bool Contains(string fieldName)
        {
            return _fieldLookup.ContainsKey(fieldName.ToLower());
        }

        public bool TryGetField(string fieldName, out Field field)
        {
            return _fieldLookup.TryGetValue(fieldName.ToLower(), out field);
        }

        public bool TryGetFieldAlias(string fieldName, out Field field)
        {
            return _fieldAliasLookup.TryGetValue(fieldName.ToLower(), out field);
        }

        /// <summary>
        /// Returns an enumerator which enumerates over the fields of this record schema
        /// </summary>
        /// <returns>Enumerator over the field in the order of their definition</returns>
        public IEnumerator<Field> GetEnumerator()
        {
            return Fields.GetEnumerator();
        }

        /// <summary>
        /// Writes the records schema in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the record schema</param>
        protected internal override void WriteJsonFields(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            base.WriteJsonFields(writer, names, encspace);

            // we allow reading for empty fields, so writing of records with empty fields are allowed as well
            if (_request)
            {
                writer.WritePropertyName("request");
            }
            else
            {
                writer.WritePropertyName("fields");
            }
            writer.WriteStartArray();

            if (null != Fields && Fields.Count > 0)
            {
                foreach (Field field in this)
                {
                    field.WriteJson(writer, names, Namespace); // use the namespace of the record for the fields
                }
            }
            writer.WriteEndArray();
        }

        /// <summary>
        /// Compares equality of two record schemas
        /// </summary>
        /// <param name="obj">record schema to compare against this schema</param>
        /// <returns>true if the two schemas are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj is RecordSchema)
            {
                RecordSchema that = obj as RecordSchema;
                return Protect(() => true, () =>
                {
                    if (SchemaName.Equals(that.SchemaName) && Count == that.Count)
                    {
                        for (int i = 0; i < Fields.Count; i++)
                        {
                            if (!Fields[i].Equals(that.Fields[i]))
                            {
                                return false;
                            }
                        }
                        return ObjectUtils.AreEqual(that.Props, Props);
                    }
                    return false;
                }, that);
            }
            return false;
        }

        /// <summary>
        /// Hash code function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Protect(() => 0, () =>
            {
                long result = SchemaName.GetHashCode() + Fields.Sum(f => 29L * f.GetHashCode());
                result += ObjectUtils.GetHashCode(Props);
                return (int)result;
            }, this);
        }

        private class RecordSchemaPair
        {
            public readonly RecordSchema first;
            public readonly RecordSchema second;

            public RecordSchemaPair(RecordSchema first, RecordSchema second)
            {
                this.first = first;
                this.second = second;
            }
        }

        [ThreadStatic]
        private static List<RecordSchemaPair> seen;

        /**
         * We want to protect against infinite recursion when the schema is recursive. We look into a thread local
         * to see if we have been into this if so, we execute the bypass function otherwise we execute the main function.
         * Before executing the main function, we ensure that we create a marker so that if we come back here recursively
         * we can detect it.
         * 
         * The infinite loop happens in ToString(), Equals() and GetHashCode() methods.
         * Though it does not happen for CanRead() because of the current implemenation of UnionSchema's can read,
         * it could potenitally happen.
         * We do a linear seach for the marker as we don't expect the list to be very long.
         */

        private T Protect<T>(Func<T> bypass, Func<T> main, RecordSchema that)
        {
            if (seen == null)
            {
                seen = new List<RecordSchemaPair>();
            }

            else if (seen.FirstOrDefault((rs => rs.first == this && rs.second == that)) != null)
            {
                return bypass();
            }

            var p = new RecordSchemaPair(this, that);
            seen.Add(p);
            try
            {
                return main();
            }
            finally
            {
                seen.Remove(p);
            }
        }
    }
}
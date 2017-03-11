using System.Collections.Generic;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// A class that contains a list of named schemas. This is used when reading or writing a schema/protocol.
    /// This prevents reading and writing of duplicate schema definitions within a protocol or schema file
    /// </summary>
    public class SchemaNames
    {
        /// <summary>
        /// Map of schema name and named schema objects
        /// </summary>
        public IDictionary<SchemaName, NamedSchema> Names
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SchemaNames()
        {
            Names = new Dictionary<SchemaName, NamedSchema>();
        }

        /// <summary>
        /// Checks if given name is in the map
        /// </summary>
        /// <param name="name">schema name</param>
        /// <returns>true or false</returns>
        public bool Contains(SchemaName name)
        {
            return Names.ContainsKey(name);
        }

        /// <summary>
        /// Adds a schema name to the map if it doesn't exist yet
        /// </summary>
        /// <param name="name">schema name</param>
        /// <param name="schema">schema object</param>
        /// <returns>true if schema was added to the list, false if schema is already in the list</returns>
        public bool Add(SchemaName name, NamedSchema schema)
        {
            if (Names.ContainsKey(name))
            {
                return false;
            }
            Names.Add(name, schema);
            return true;
        }

        /// <summary>
        /// Adds a named schema to the list
        /// </summary>
        /// <param name="schema">schema object</param>
        /// <returns>true if schema was added to the list, false if schema is already in the list</returns>
        public bool Add(NamedSchema schema)
        {
            return Add(schema.SchemaName, schema);
        }

        /// <summary>
        /// Tries to get the value for the given name fields
        /// </summary>
        /// <param name="name">name of the schema</param>
        /// <param name="space">namespace of the schema</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        /// <param name="schema">schema object found</param>
        /// <returns>true if name is found in the map, false otherwise</returns>
        public bool TryGetValue(string name, string space, string encspace, out NamedSchema schema)
        {
            var schemaName = new SchemaName(name, space, encspace);
            return Names.TryGetValue(schemaName, out schema);
        }

        /// <summary>
        /// Returns the enumerator for the map
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<SchemaName, NamedSchema>> GetEnumerator()
        {
            return Names.GetEnumerator();
        }
    }
}
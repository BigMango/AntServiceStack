using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class to store schema name, namespace and enclosing namespace
    /// </summary>
    public class SchemaName
    {
        /// <summary>
        /// Name of the schema
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Namespace specified within the schema
        /// </summary>
        public string Space
        {
            get;
            private set;
        }

        /// <summary>
        /// Namespace from the most tightly enclosing schema
        /// </summary>
        public string EncSpace
        {
            get;
            private set;
        }

        /// <summary>
        /// Namespace.Name of the schema
        /// </summary>
        public string Fullname
        {
            get
            {
                return string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        /// <summary>
        /// Namespace of the schema
        /// </summary>
        public string Namespace
        {
            get
            {
                return string.IsNullOrEmpty(Space) ? EncSpace : Space;
            }
        }

        /// <summary>
        /// Constructor for SchemaName
        /// </summary>
        /// <param name="name">name of the schema</param>
        /// <param name="space">namespace of the schema</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        public SchemaName(string name, string space, string encspace)
        {
            if (name == null)
            {
                // anonymous
                Name = Space = null;
                EncSpace = encspace;
                    // need to save enclosing namespace for anonymous types, so named types within the anonymous type can be resolved
            }
            else if (!name.Contains("."))
            {
                // unqualified name
                Space = space; // use default space
                Name = name;
                EncSpace = encspace;
            }
            else
            {
                string[] parts = name.Split('.');
                Space = string.Join(".", parts, 0, parts.Length - 1);
                Name = parts[parts.Length - 1];
                EncSpace = encspace;
            }
        }

        /// <summary>
        /// Returns the full name of the schema
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Fullname;
        }

        /// <summary>
        /// Writes the schema name in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schemas already written</param>
        /// <param name="encspace">enclosing namespace of the schema</param>
        internal void WriteJson(JsonTextWriter writer, SchemaNames names, string encspace)
        {
            if (null == Name)
            {
                return;
            }
            JsonHelper.WriteIfNotNullOrEmpty(writer, "name", Name);
            if (!string.IsNullOrEmpty(Space))
            {
                JsonHelper.WriteIfNotNullOrEmpty(writer, "namespace", Space);
            }
            else if (!string.IsNullOrEmpty(EncSpace)) // need to put enclosing name space for code generated classes
            {
                JsonHelper.WriteIfNotNullOrEmpty(writer, "namespace", EncSpace);
            }
        }

        /// <summary>
        /// Compares two schema names
        /// </summary>
        /// <param name="obj">SchameName object to compare against this object</param>
        /// <returns>true or false</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            var that = (SchemaName)obj;
            if (that == null)
            {
                return false;
            }
            return that.Name == Name && that.Namespace == Namespace;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Fullname) ? 0 : 29 * Fullname.GetHashCode();
        }
    }
}
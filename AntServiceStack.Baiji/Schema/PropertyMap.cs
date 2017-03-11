using System.Collections.Generic;
using AntServiceStack.Baiji.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    public class PropertyMap : Dictionary<string, string>
    {
        /// <summary>
        /// Set of reserved schema property names, any other properties not defined in this set are custom properties and can be added to this map
        /// </summary>
        private static readonly List<string> ReservedProps = new List<string>
        {
            "type",
            "name",
            "namespace",
            "fields",
            "items",
            "size",
            "symbols",
            "values",
            "aliases",
            "order",
            "doc",
            "default"
        };

        /// <summary>
        /// Parses the custom properties from the given JSON object and stores them
        /// into the schema's list of custom properties
        /// </summary>
        /// <param name="jtok">JSON object to prase</param>
        public void Parse(JToken jtok)
        {
            JObject jo = jtok as JObject;
            foreach (JProperty prop in jo.Properties())
            {
                if (ReservedProps.Contains(prop.Name))
                {
                    continue;
                }
                if (!ContainsKey(prop.Name))
                {
                    Add(prop.Name, prop.Value.ToString());
                }
            }
        }

        /// <summary>
        /// Adds a custom property to the schema
        /// </summary>
        /// <param name="key">custom property name</param>
        /// <param name="value">custom property value</param>
        public void Set(string key, string value)
        {
            if (ReservedProps.Contains(key))
            {
                throw new BaijiException("Can't set reserved property: " + key);
            }

            string oldValue;
            if (!TryGetValue(key, out oldValue))
            {
                Add(key, value);
            }
            else if (!oldValue.Equals(value))
            {
                throw new BaijiException("Property cannot be overwritten: " + key);
            }
        }

        /// <summary>
        /// Writes the schema's custom properties in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        public void WriteJson(JsonTextWriter writer)
        {
            foreach (KeyValuePair<string, string> kp in this)
            {
                if (ReservedProps.Contains(kp.Key))
                {
                    continue;
                }

                writer.WritePropertyName(kp.Key);
                writer.WriteValue(kp.Value);
            }
        }

        /// <summary>
        /// Function to compare equality of two PropertyMaps
        /// </summary>
        /// <param name="obj">other PropertyMap</param>
        /// <returns>true if contents of the two maps are the same, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is PropertyMap))
            {
                return false;
            }

            var that = obj as PropertyMap;
            if (Count != that.Count)
            {
                return false;
            }
            foreach (KeyValuePair<string, string> pair in this)
            {
                if (!that.ContainsKey(pair.Key))
                {
                    return false;
                }
                if (!pair.Value.Equals(that[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Hashcode function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = Count;
            int index = 1;
            foreach (var pair in this)
            {
                hash += (pair.Key.GetHashCode() + pair.Value.GetHashCode()) * index++;
            }
            return hash;
        }
    }
}
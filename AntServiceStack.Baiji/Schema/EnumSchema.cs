using System.Collections.Generic;
using System.Linq;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.Schema
{
    /// <summary>
    /// Class for enum type schemas
    /// </summary>
    public class EnumSchema : NamedSchema
    {
        /// <summary>
        /// List of strings representing the enum symbols
        /// </summary>
        public IList<string> Symbols
        {
            get;
            private set;
        }

        /// <summary>
        /// Map of enum symbols and it's corresponding ordinal number
        /// The first element of value is the explicit value which can be null, the second one is the actual value.
        /// </summary>
        private readonly IDictionary<string, int?[]> symbolMap;

        /// <summary>
        /// Count of enum symbols
        /// </summary>
        public int Count
        {
            get
            {
                return Symbols.Count;
            }
        }

        /// <summary>
        /// Static function to return new instance of EnumSchema
        /// </summary>
        /// <param name="token">JSON object for enum schema</param>
        /// <param name="names">list of named schema already parsed in</param>
        /// <param name="encspace">enclosing namespace for the enum schema</param>
        /// <returns>new instance of enum schema</returns>
        internal static EnumSchema NewInstance(JToken token, PropertyMap props, SchemaNames names, string encspace)
        {
            SchemaName name = GetName(token, encspace);
            var aliases = GetAliases(token, name.Space, name.EncSpace);
            var doc = JsonHelper.GetOptionalString(token, "doc");

            JArray jsymbols = token["symbols"] as JArray;
            if (null == jsymbols)
            {
                throw new SchemaParseException("Enum has no symbols: " + name);
            }

            var symbols = new List<string>();
            IDictionary<string, int?[]> symbolMap = new Dictionary<string, int?[]>();
            int lastValue = -1;
            foreach (JToken jsymbol in jsymbols)
            {
                int? explicitValue = null;
                int actualValue;
                string symbol;
                if (jsymbol is JValue)
                {
                    symbol = (string)((JValue)jsymbol).Value;
                    actualValue = ++lastValue;
                }
                else if (jsymbol is JObject)
                {
                    var symbolObj = (JObject)jsymbol;
                    symbol = symbolObj.Value<string>("name");
                    if (symbol == null)
                    {
                        throw new SchemaParseException("Missing symbol name: " + jsymbol);
                    }
                    JToken valueToken;
                    if (symbolObj.TryGetValue("value", out valueToken))
                    {
                        try
                        {
                            explicitValue = symbolObj.Value<int>("value");
                        }
                        catch (System.Exception)
                        {
                            throw new SchemaParseException("Only integer value is allowed for an enum symbol: " + jsymbol);
                        }
                    }
                    lastValue = actualValue = explicitValue.HasValue ? explicitValue.Value : lastValue + 1;
                }
                else
                {
                    throw new SchemaParseException("Invalid symbol object: " + jsymbol);
                }
               
                if (symbolMap.ContainsKey(symbol))
                {
                    throw new SchemaParseException("Duplicate symbol: " + symbol);
                }

                symbolMap[symbol] = new [] {explicitValue, actualValue};
                symbols.Add(symbol);
            }
            return new EnumSchema(name, doc, aliases, symbols, symbolMap, props, names);
        }

        /// <summary>
        /// Constructor for enum schema
        /// </summary>
        /// <param name="name">name of enum</param>
        /// <param name="doc"></param>
        /// <param name="aliases">list of aliases for the name</param>
        /// <param name="symbols">
        /// list of enum symbols, Map of enum symbols and it's corresponding ordinal number
        /// The first element of value is the explicit value which can be null, the second one is the actual value.
        /// </param>
        /// <param name="props"></param>
        public EnumSchema(SchemaName name, string doc, IList<SchemaName> aliases, KeyValuePair<string, int?>[] symbols,
            PropertyMap props)
            : base(SchemaType.Enumeration, name, doc, aliases, props, new SchemaNames())
        {
            if (null == name.Name)
            {
                throw new SchemaParseException("name cannot be null for enum schema.");
            }
            Symbols = symbols.Select(s => s.Key).ToList();
            var symbolMap = new Dictionary<string, int?[]>();
            int lastValue = -1;
            foreach (var symbol in symbols)
            {
                int?[] values = new int?[2];
                if (symbol.Value.HasValue)
                {
                    values[0] = values[1] = lastValue = symbol.Value.Value;
                }
                else
                {
                    values[1] = ++lastValue;
                }
                symbolMap[symbol.Key] = values;
            }
            this.symbolMap = symbolMap;
        }

        /// <summary>
        /// Constructor for enum schema
        /// </summary>
        /// <param name="name">name of enum</param>
        /// <param name="doc"></param>
        /// <param name="aliases">list of aliases for the name</param>
        /// <param name="symbols">list of enum symbols</param>
        /// <param name="symbolMap">map of enum symbols and value</param>
        /// <param name="names">list of named schema already read</param>
        private EnumSchema(SchemaName name, string doc, IList<SchemaName> aliases, IList<string> symbols,
            IDictionary<string, int?[]> symbolMap, PropertyMap props, SchemaNames names)
            : base(SchemaType.Enumeration, name, doc, aliases, props, names)
        {
            if (null == name.Name)
            {
                throw new SchemaParseException("name cannot be null for enum schema.");
            }
            Symbols = symbols;
            this.symbolMap = symbolMap;
        }

        /// <summary>
        /// Writes enum schema in JSON format
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="names">list of named schema already written</param>
        /// <param name="encspace">enclosing namespace of the enum schema</param>
        protected internal override void WriteJsonFields(JsonTextWriter writer,
            SchemaNames names, string encspace)
        {
            base.WriteJsonFields(writer, names, encspace);
            writer.WritePropertyName("symbols");
            writer.WriteStartArray();
            foreach (string s in Symbols)
            {
                writer.WriteValue(s);
            }
            writer.WriteEndArray();
        }

        /// <summary>
        /// Returns the position of the given symbol within this enum. 
        /// Throws BaijiException if the symbol is not found in this enum.
        /// </summary>
        /// <param name="symbol">name of the symbol to find</param>
        /// <returns>position of the given symbol in this enum schema</returns>
        public int Ordinal(string symbol)
        {
            int?[] result;
            if (symbolMap.TryGetValue(symbol, out result))
            {
                return result[1].Value;
            }
            throw new BaijiException("No such symbol: " + symbol);
        }

        /// <summary>
        /// Returns the enum symbol of the given value to the list
        /// </summary>
        /// <param name="value">A symbol value</param>
        /// <returns>
        /// The symbol name corresponding to the given value.
        /// If there are multiple symbols associated with the given value, the first one in the list will be returned.
        /// </returns>
        public string GetSymbol(int value)
        {
            foreach (var pair in symbolMap)
            {
                if (pair.Value[1] == value)
                {
                    return pair.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if given symbol is in the list of enum symbols
        /// </summary>
        /// <param name="symbol">symbol to check</param>
        /// <returns>true if symbol exist, false otherwise</returns>
        public bool Contains(string symbol)
        {
            return symbolMap.ContainsKey(symbol);
        }

        /// <summary>
        /// Returns an enumerator that enumerates the symbols in this enum schema in the order of their definition.
        /// </summary>
        /// <returns>Enumeration over the symbols of this enum schema</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return Symbols.GetEnumerator();
        }

        /// <summary>
        /// Checks equality of two enum schema
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            var that = obj as EnumSchema;
            if (that == null)
            {
                return false;
            }
            if (SchemaName.Equals(that.SchemaName) && Count == that.Count)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!Symbols[i].Equals(that.Symbols[i]))
                    {
                        return false;
                    }
                }
                return ObjectUtils.AreEqual(that.Props, Props);
            }
            return false;
        }

        /// <summary>
        /// Hashcode function
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(SchemaName.GetHashCode() + ObjectUtils.GetHashCode(Props) + Symbols.Sum(s => 23L * s.GetHashCode()));
        }
    }
}
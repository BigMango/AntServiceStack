using AntServiceStack.Common.Extensions;

namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class DictionaryParser<TKey, TValue> : IValueParser<Dictionary<TKey, TValue>>
    {
        private IValueParser<TKey> keyParser;
        private IValueParser<TValue> valueParser;

        public DictionaryParser(IValueParser<TKey> keyParser, IValueParser<TValue> valueParser)
        {
            keyParser = keyParser.NotNull("keyParser");
            valueParser = valueParser.NotNull("valueParser");
            this.keyParser = keyParser;
            this.valueParser = valueParser;
        }

        public Dictionary<TKey, TValue> Parse(string input)
        {
            Dictionary<TKey, TValue> dictionary;
            if (!this.TryParse(input, out dictionary))
            {
                return null;
            }
            return dictionary;
        }

        public bool TryParse(string input, out Dictionary<TKey, TValue> result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            foreach (string str in input.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    TKey local;
                    TValue local2;
                    string[] strArray3 = str.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray3.Length != 2)
                    {
                        return false;
                    }
                    string str2 = strArray3[0].Trim();
                    string str3 = strArray3[1].Trim();
                    if (!this.keyParser.TryParse(str2, out local) || !this.valueParser.TryParse(str3, out local2))
                    {
                        return false;
                    }
                    dictionary[local] = local2;
                }
            }
            if (dictionary.Count == 0)
            {
                return false;
            }
            result = dictionary;
            return true;
        }
    }
}


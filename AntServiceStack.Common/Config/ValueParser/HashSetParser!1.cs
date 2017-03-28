using AntServiceStack.Common.Extensions;

namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class HashSetParser<T> : IValueParser<HashSet<T>>
    {
        private IValueParser<T> valueParser;

        public HashSetParser(IValueParser<T> valueParser)
        {
            valueParser = valueParser.NotNull("valueParser");
            this.valueParser = valueParser;
        }

        public HashSet<T> Parse(string input)
        {
            HashSet<T> set;
            if (!this.TryParse(input, out set))
            {
                return null;
            }
            return set;
        }

        public bool TryParse(string input, out HashSet<T> result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            HashSet<T> set = new HashSet<T>();
            foreach (string str in input.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    T local;
                    if (!this.valueParser.TryParse(str.Trim(), out local))
                    {
                        return false;
                    }
                    set.Add(local);
                }
            }
            if (set.Count == 0)
            {
                return false;
            }
            result = set;
            return true;
        }
    }
}


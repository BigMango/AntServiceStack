using AntServiceStack.Common.Extensions;

namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class ListParser<T> : IValueParser<List<T>>
    {
        private IValueParser<T> valueParser;

        public ListParser(IValueParser<T> valueParser)
        {
            valueParser = valueParser.NotNull("valueParser");
            this.valueParser = valueParser;
        }

        public List<T> Parse(string input)
        {
            List<T> list;
            if (!this.TryParse(input, out list))
            {
                return null;
            }
            return list;
        }

        public bool TryParse(string input, out List<T> result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            List<T> list = new List<T>();
            foreach (string str in input.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    T local;
                    if (!this.valueParser.TryParse(str.Trim(), out local))
                    {
                        return false;
                    }
                    list.Add(local);
                }
            }
            if (list.Count == 0)
            {
                return false;
            }
            result = list;
            return true;
        }
    }
}


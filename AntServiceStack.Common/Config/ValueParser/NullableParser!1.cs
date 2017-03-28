using AntServiceStack.Common.Extensions;

namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class NullableParser<T> : IValueParser<T?> where T: struct
    {
        private IValueParser<T> valueParser;

        public NullableParser(IValueParser<T> valueParser)
        {
            valueParser = valueParser.NotNull( "valueParser");
            this.valueParser = valueParser;
        }

        public T? Parse(string input)
        {
            return new T?(this.valueParser.Parse(input));
        }

        public bool TryParse(string input, out T? result)
        {
            T local;
            bool flag = this.valueParser.TryParse(input, out local);
            result = new T?(flag ? local : default(T));
            return flag;
        }
    }
}


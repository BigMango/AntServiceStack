namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class LongParser : IValueParser<long>
    {
        public static readonly LongParser Instance = new LongParser();

        public long Parse(string value)
        {
            return long.Parse(value);
        }

        public bool TryParse(string input, out long result)
        {
            return long.TryParse(input, out result);
        }
    }
}


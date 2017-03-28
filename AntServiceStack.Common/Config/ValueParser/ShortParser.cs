namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class ShortParser : IValueParser<short>
    {
        public static readonly ShortParser Instance = new ShortParser();

        public short Parse(string value)
        {
            return short.Parse(value);
        }

        public bool TryParse(string input, out short result)
        {
            return short.TryParse(input, out result);
        }
    }
}


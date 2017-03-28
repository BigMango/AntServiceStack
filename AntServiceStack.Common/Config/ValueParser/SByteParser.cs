namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class SByteParser : IValueParser<sbyte>
    {
        public static readonly SByteParser Instance = new SByteParser();

        public sbyte Parse(string value)
        {
            return sbyte.Parse(value);
        }

        public bool TryParse(string input, out sbyte result)
        {
            return sbyte.TryParse(input, out result);
        }
    }
}


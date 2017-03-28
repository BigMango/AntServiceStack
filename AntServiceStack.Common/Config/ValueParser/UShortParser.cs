namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class UShortParser : IValueParser<ushort>
    {
        public static readonly UShortParser Instance = new UShortParser();

        public ushort Parse(string value)
        {
            return ushort.Parse(value);
        }

        public bool TryParse(string input, out ushort result)
        {
            return ushort.TryParse(input, out result);
        }
    }
}


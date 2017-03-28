namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class ByteParser : IValueParser<byte>
    {
        public static readonly ByteParser Instance = new ByteParser();

        public byte Parse(string value)
        {
            return byte.Parse(value);
        }

        public bool TryParse(string input, out byte result)
        {
            return byte.TryParse(input, out result);
        }
    }
}


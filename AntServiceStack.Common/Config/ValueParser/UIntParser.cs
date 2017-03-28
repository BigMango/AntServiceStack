namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class UIntParser : IValueParser<uint>
    {
        public static readonly UIntParser Instance = new UIntParser();

        public uint Parse(string value)
        {
            return uint.Parse(value);
        }

        public bool TryParse(string input, out uint result)
        {
            return uint.TryParse(input, out result);
        }
    }
}


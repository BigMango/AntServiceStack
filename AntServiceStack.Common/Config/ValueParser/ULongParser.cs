namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class ULongParser : IValueParser<ulong>
    {
        public static readonly ULongParser Instance = new ULongParser();

        public ulong Parse(string value)
        {
            return ulong.Parse(value);
        }

        public bool TryParse(string input, out ulong result)
        {
            return ulong.TryParse(input, out result);
        }
    }
}


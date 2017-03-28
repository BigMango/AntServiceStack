namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class IntParser : IValueParser<int>
    {
        public static readonly IntParser Instance = new IntParser();

        public int Parse(string value)
        {
            return int.Parse(value);
        }

        public bool TryParse(string input, out int result)
        {
            return int.TryParse(input, out result);
        }
    }
}


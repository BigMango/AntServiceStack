namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class StringParser : IValueParser<string>
    {
        public static readonly StringParser Instance = new StringParser();

        public string Parse(string value)
        {
            return value;
        }

        public bool TryParse(string input, out string result)
        {
            result = input;
            return true;
        }
    }
}


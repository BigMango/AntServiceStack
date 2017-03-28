namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class FloatParser : IValueParser<float>
    {
        public static readonly FloatParser Instance = new FloatParser();

        public float Parse(string value)
        {
            return float.Parse(value);
        }

        public bool TryParse(string input, out float result)
        {
            return float.TryParse(input, out result);
        }
    }
}


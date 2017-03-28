namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class DoubleParser : IValueParser<double>
    {
        public static readonly DoubleParser Instance = new DoubleParser();

        public double Parse(string value)
        {
            return double.Parse(value);
        }

        public bool TryParse(string input, out double result)
        {
            return double.TryParse(input, out result);
        }
    }
}


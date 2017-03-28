namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class DecimalParser : IValueParser<decimal>
    {
        public static readonly DecimalParser Instance = new DecimalParser();

        public decimal Parse(string value)
        {
            return decimal.Parse(value);
        }

        public bool TryParse(string input, out decimal result)
        {
            return decimal.TryParse(input, out result);
        }
    }
}


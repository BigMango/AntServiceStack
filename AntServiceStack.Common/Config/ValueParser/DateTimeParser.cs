namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class DateTimeParser : IValueParser<DateTime>
    {
        public static readonly DateTimeParser Instance = new DateTimeParser();

        public DateTime Parse(string value)
        {
            return DateTime.Parse(value);
        }

        public bool TryParse(string input, out DateTime result)
        {
            return DateTime.TryParse(input, out result);
        }
    }
}


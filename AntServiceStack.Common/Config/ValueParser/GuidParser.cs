namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class GuidParser : IValueParser<Guid>
    {
        public static readonly GuidParser Instance = new GuidParser();

        public Guid Parse(string value)
        {
            return Guid.Parse(value);
        }

        public bool TryParse(string input, out Guid result)
        {
            return Guid.TryParse(input, out result);
        }
    }
}


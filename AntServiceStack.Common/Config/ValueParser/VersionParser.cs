namespace AntServiceStack.Common.Config.ValueParser
{
    using System;
    using System.Runtime.InteropServices;

    public class VersionParser : IValueParser<Version>
    {
        public static readonly VersionParser Instance = new VersionParser();

        public Version Parse(string value)
        {
            return Version.Parse(value);
        }

        public bool TryParse(string input, out Version result)
        {
            return Version.TryParse(input, out result);
        }
    }
}


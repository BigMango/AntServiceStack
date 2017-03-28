
namespace AntServiceStack.Common.Config.ValueParser
{ 
    public class BoolParser : IValueParser<bool>
    {
        public static readonly BoolParser Instance = new BoolParser();

        public bool Parse(string value)
        {
            return bool.Parse(value);
        }

        public bool TryParse(string input, out bool result)
        {
            return bool.TryParse(input, out result);
        }
    }
}


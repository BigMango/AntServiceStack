namespace AntServiceStack.Common.Config.ValueParser
{


    public class CharParser : IValueParser<char>
    {
        public static readonly CharParser Instance = new CharParser();

        public char Parse(string value)
        {
            return char.Parse(value);
        }

        public bool TryParse(string input, out char result)
        {
            return char.TryParse(input, out result);
        }
    }
}


using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class StringMacro : IMacro
    {
        private readonly Regex _regex;

        public string Pattern => @"""((?:\\.|[^\\""])*)""";

        public bool AreArgumentsRequired => false;

        public StringMacro()
        {
            _regex = new Regex(Pattern);
        }

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            int position = reader.Current.CurrentAbsolutePosition;
            int column = reader.Current.CurrentPosition;
            string remainder = reader.Current.Remainder;

            string quoted = _regex.Match(remainder).Value;
            reader.Current.Advance(quoted.Length);

            return Task.FromResult((new List<TextBlock>() { new TextBlock(quoted) { Column = column, Position = position, Line = reader.LineNumber }}, true));
        }
    }
}

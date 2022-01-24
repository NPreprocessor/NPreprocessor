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
            string remainder = reader.Current.Remainder;
            string quoted = _regex.Match(remainder).Value;
            reader.Current.Consume(quoted.Length);

            return Task.FromResult((new List<TextBlock>() { quoted }, true));
        }
    }
}

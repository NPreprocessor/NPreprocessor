using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class StringMacro : IMacro
    {
        public string Pattern => @"""((?:\\.|[^\\""])*)""";

        public bool AreArgumentsRequired => false;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            string remainder = reader.Current.Remainder;
            string quoted = Regex.Match(remainder, Pattern).Value;

            reader.Current.Consume(quoted.Length);

            return Task.FromResult((new List<TextBlock>() { quoted }, true));
        }
    }
}

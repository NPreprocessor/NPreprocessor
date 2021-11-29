using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class StringMacro : IMacro
    {
        public string Pattern => @"""((?:\\.|[^\\""])*)""";

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            string remainder = reader.Current.Remainder;

            string quoted = Regex.Match(remainder, Pattern).Value;

            reader.Current.Consume(quoted.Length);

            return (new List<string>() { quoted }, true);
        }
    }
}

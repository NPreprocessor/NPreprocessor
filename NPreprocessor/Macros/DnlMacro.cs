using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Prefix => "dnl";

        private Regex _regex = new Regex($@"\bdnl\b.*", RegexOptions.Singleline);

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            if (_regex.IsMatch(reader.Current))
            {
                var result = _regex.Replace(reader.Current, string.Empty);

                reader.Finish();
                state.MergePoints += 2;

                return (new List<string>() { result }, true);
            }
            return (null, false);
        }

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }
            return Regex.IsMatch(currentLine.Current, $@"\b{Prefix}\b");
        }
    }
}

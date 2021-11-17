using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Prefix => "dnl";

        private Regex _regex = new Regex($@"\bdnl\b.*", RegexOptions.Singleline);

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            if (_regex.IsMatch(txtReader.Current.Remainder))
            {
                var result = _regex.Replace(txtReader.Current.Remainder, string.Empty);

                txtReader.Current.Finish();
                state.MergePoints += 2;

                return (new List<string>() { result }, true);
            }
            return (null, false);
        }

        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, @$"^{Prefix}\b");
            }
            return Regex.IsMatch(txtReader.Current.Remainder, $@"\b{Prefix}\b");
        }
    }
}

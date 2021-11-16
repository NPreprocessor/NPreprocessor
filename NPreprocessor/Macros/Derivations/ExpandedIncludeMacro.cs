using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIncludeMacro : IMacro
    {
        public ExpandedIncludeMacro(string prefixName)
        {
            Prefix = prefixName;
        }

        public string Prefix { get; set; }

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var line = txtReader.Current.Remainder;
            var prefixLength = Prefix.Length;
            var fileName = line.Substring(prefixLength).Trim().Trim('\"');
            txtReader.Current.Finish();

            var m4Line = $"include(`{fileName}')";
            return (new List<string>() { m4Line }, true);
        }


        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, $"^{Prefix}\b");
            }

            return Regex.IsMatch(txtReader.Current.Remainder, $@"{Prefix}");
        }
    }
}

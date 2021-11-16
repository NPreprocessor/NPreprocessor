using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedUndefineMacro : IMacro
    {
        public ExpandedUndefineMacro(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; set; }

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var line = txtReader.Current.Remainder;
            txtReader.Current.Finish();
            var prefixLength = Prefix.Length;
            var name = line.Substring(prefixLength).TrimEnd('\r').TrimEnd('\n');

            var m4Line = "undefine(`" + name + "')";
            return (new List<string>() { m4Line }, true);
        }

        public (string result, bool resolved) Resolve(string line)
        {
            return (line, false);
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
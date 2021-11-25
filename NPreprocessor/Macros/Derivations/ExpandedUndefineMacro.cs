using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedUndefineMacro : IMacro
    {
        private readonly string defPrefix;

        public ExpandedUndefineMacro(string prefix, string defPrefix = "")
        {
            Pattern = prefix;
            this.defPrefix = defPrefix;
        }

        public string Pattern { get; set; }

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            var line = txtReader.Current.Remainder;
            txtReader.Current.Finish();
            var prefixLength = Pattern.Length;
            var name = line.Substring(prefixLength).TrimEnd('\r').TrimEnd('\n').Trim();

            var m4Line = "undefine(`" + defPrefix + name + "')";
            return (new List<string>() { m4Line }, false);
        }
    }
}
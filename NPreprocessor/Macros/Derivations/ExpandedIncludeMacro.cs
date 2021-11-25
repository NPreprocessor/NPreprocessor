using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIncludeMacro : IMacro
    {
        public ExpandedIncludeMacro(string prefixName)
        {
            Pattern = prefixName;
        }

        public string Pattern { get; set; }

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            var line = txtReader.Current.Remainder;
            var prefixLength = Pattern.Length;
            var fileName = MacroString.Trim(line.Substring(prefixLength).Trim().Trim('\"'));
            txtReader.Current.Finish();

            var m4Line = $"include(`{fileName}')";
            return (new List<string>() { m4Line }, false);
        }
    }
}

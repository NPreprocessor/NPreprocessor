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

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var line = reader.Current;
            var prefixLength = Prefix.Length;
            var fileName = line.Substring(prefixLength).Trim().Trim('\"');
            reader.Finish();

            var m4Line = $"include(`{fileName}')";
            return (new List<string>() { m4Line }, true);
        }


        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }

            return Regex.IsMatch(currentLine.Current, $@"{Prefix}");
        }
    }
}

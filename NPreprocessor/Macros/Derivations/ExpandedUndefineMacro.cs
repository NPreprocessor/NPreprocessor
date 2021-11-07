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

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var line = reader.Current;
            reader.Finish();
            var prefixLength = Prefix.Length;
            var name = line.Substring(prefixLength).TrimEnd('\r').TrimEnd('\n');

            var m4Line = "undefine(`" + name + "')";
            return (new List<string>() { m4Line }, true);
        }

        public (string result, bool resolved) Resolve(string line)
        {
            return (line, false);
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
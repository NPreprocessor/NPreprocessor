using System.Collections.Generic;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedUndefineMacro : IMacro
    {
        public ExpandedUndefineMacro(string prefix)
        {
            Pattern = prefix;
        }

        public string Pattern { get; set; }

        public bool AreArgumentsRequired => false;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var line = reader.Current.Remainder;
            reader.Current.Finish(keapNewLine: true);
            var prefixLength = Pattern.Length;
            var name = line.Substring(prefixLength).TrimEnd('\r').TrimEnd('\n').Trim();

            var m4Line = "undefine(`" + state.DefinitionPrefix + name + "')";
            return (new List<TextBlock>() { m4Line }, false);
        }
    }
}
using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            int position = reader.Current.CurrentAbsolutePosition;
            int column = reader.Current.CurrentPosition;
            var line = reader.Current.Remainder;

            reader.Current.Finish(keapNewLine: true);
            var prefixLength = Pattern.Length;
            var name = line.Substring(prefixLength).TrimEnd('\r').TrimEnd('\n').Trim();

            var m4Line = "undefine(`" + state.DefinitionPrefix + name + "')";
            return Task.FromResult((new List<TextBlock>() { new TextBlock(m4Line) { Column = column, Position = position, Line = reader.Current.LineNumber }}, false));
        }
    }
}
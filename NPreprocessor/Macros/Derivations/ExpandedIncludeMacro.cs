using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            int lineNumber = reader.Current.LineNumber;
            int columnNumber = reader.Current.ColumnNumber;

            var line = reader.Current.Remainder;
            var prefixLength = Pattern.Length;
            var fileName = MacroString.Trim(line.Substring(prefixLength).Trim().Trim('\"'));
            reader.Current.Finish(keepNewLine: true);

            var m4Line = $"include(`{fileName}')";
            return Task.FromResult(new List<TextBlock>() { new TextBlock(m4Line) { Column = columnNumber, Line = lineNumber } });
        }
    }
}

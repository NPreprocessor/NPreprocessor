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

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            var line = reader.Current.Remainder;
            var prefixLength = Pattern.Length;
            var fileName = MacroString.Trim(line.Substring(prefixLength).Trim().Trim('\"'));
            reader.Current.Finish(keapNewLine: true);

            var m4Line = $"include(`{fileName}')";
            return Task.FromResult((new List<TextBlock>() { m4Line }, false));
        }
    }
}

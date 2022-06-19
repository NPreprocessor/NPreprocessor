using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class IfNDefMacro : IMacro
    {
        public IfNDefMacro()
        {
        }

        public string Pattern => "ifndef";

        public bool AreArgumentsRequired => true;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            int lineNumber = reader.Current.LineNumber;

            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Advance(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (!state.Definitions.Contains(name))
            {
                return Task.FromResult(MacroString.GetBlocks(args[1], state.NewLineEnding, args.Length >= 4 ? int.Parse(args[3]) : lineNumber ));
            }
            else
            {
                if (args.Length >= 3)
                {
                    return Task.FromResult(MacroString.GetBlocks(args[2], state.NewLineEnding, args.Length >= 4 ? int.Parse(args[4]) : lineNumber));
                }
                else
                {
                    return Task.FromResult(new List<TextBlock>());
                }
            }
        }
    }
}

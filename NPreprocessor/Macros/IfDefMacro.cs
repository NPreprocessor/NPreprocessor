using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class IfDefMacro : IMacro
    {
        public IfDefMacro()
        {
        }

        public string Pattern => "ifdef";

        public bool AreArgumentsRequired => true;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (state.Definitions.Contains(name))
            {
                return (MacroString.GetBlocks(args[1], state.NewLineEnding), false);
            }
            else
            {
                if (args.Length == 3)
                {
                    return (MacroString.GetBlocks(args[2], state.NewLineEnding), false);
                }
                else
                {
                    return (new List<TextBlock>(), true);
                }
            }
        }
    }
}

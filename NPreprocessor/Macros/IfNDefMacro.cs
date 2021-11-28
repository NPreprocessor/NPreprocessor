using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class IfNDefMacro : IMacro
    {
        public IfNDefMacro()
        {
        }

        public string Pattern => "ifndef";

        public bool AreArgumentsRequired => true;
        
        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (!state.Definitions.Contains(name))
            {
                return (MacroString.GetLines(args[1]), true);
            }
            else
            {
                if (args.Length == 3)
                {
                    return (MacroString.GetLines(args[2]), true);
                }
                else
                {
                    return (new List<string>(), true);
                }
            }
        }
    }
}

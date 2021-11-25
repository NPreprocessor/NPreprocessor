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

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(txtReader, 0, state.Definitions);
            txtReader.Current.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (state.Definitions.Contains(name))
            {
                return (MacroString.GetLines(args[1]), false);
            }
            else
            {
                if (args.Length == 3)
                {
                    return (MacroString.GetLines(args[2]), false);
                }
                else
                {
                    return (new List<string>(), true);
                }
            }
        }
    }
}

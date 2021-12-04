using System;
using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class RegexMacro : IMacro
    {
        public RegexMacro()
        {
        }

        public string Pattern { get; } = "regex";

        public bool AreArgumentsRequired => true;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);

            if (call.name == null)
            {
                return (null, false);
            }

            var args = call.args;

            if (args.Length != 2)
            {
                throw new System.Exception("Invalid regex");
            }

            var name = MacroString.Trim(args[0]);
            var value = MacroString.Trim(args[1]);

            state.Regexes[name] = value;

            reader.Current.Consume(call.length);
            return (new List<TextBlock> { }, true);
        }
    }
}

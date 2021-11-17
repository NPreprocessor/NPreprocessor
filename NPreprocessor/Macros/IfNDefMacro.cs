using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class IfNDefMacro : IMacro
    {
        private readonly DefineMacro defineMacro;

        public IfNDefMacro(DefineMacro defineMacro)
        {
            this.defineMacro = defineMacro;
        }

        public string Prefix => "ifndef";

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(txtReader, 0, state.Definitions);
            txtReader.Current.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (!defineMacro.IsDefined(name))
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


        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, $"^{Prefix}");
            }
            return Regex.IsMatch(txtReader.Current.Remainder, $@"\b{Prefix}");
        }
    }
}

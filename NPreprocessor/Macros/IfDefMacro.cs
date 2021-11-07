using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class IfDefMacro : IMacro
    {
        private readonly DefineMacro defineMacro;

        public IfDefMacro(DefineMacro defineMacro)
        {
            this.defineMacro = defineMacro;
        }

        public string Prefix => "ifdef";

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}");
            }
            return Regex.IsMatch(currentLine.Current, $@"\b{Prefix}");
        }

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var line = reader.Current;

            var call = CallParser.GetInvocation(line);
            reader.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (defineMacro.IsDefined(name))
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

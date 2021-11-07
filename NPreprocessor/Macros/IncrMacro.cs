using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class IncrMacro : AritimeticMacroBase, IMacro
    {
        public IncrMacro(DefineMacro defineMacro) : base(defineMacro)
        {
        }

        public string Prefix => "incr";

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, @$"^{Prefix}\(");
            }
            return Regex.IsMatch(currentLine.Current, $@"\b{Prefix}\(");
        }

        public (List<string> result, bool invoked) Invoke(ILineReader currentLineReader, ITextReader reader, State state)
        {
            return base.Invoke(currentLineReader, reader, state, 1);
        }
    }
}

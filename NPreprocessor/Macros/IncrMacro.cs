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

        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, @$"^{Prefix}\(");
            }
            return Regex.IsMatch(txtReader.Current.Remainder, $@"\b{Prefix}\(");
        }

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            return base.Invoke(txtReader, state, 1);
        }
    }
}

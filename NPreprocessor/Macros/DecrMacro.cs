using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class DecrMacro : AritimeticMacroBase, IMacro
    {
        public DecrMacro(DefineMacro defineMacro) : base(defineMacro)
        {
        }

        public string Prefix => "decr";

        public bool CanBeUsed(ITextReader reader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(reader.Current.Remainder, @$"^{Prefix}\(");
            }
            return Regex.IsMatch(reader.Current.Remainder, $@"\b{Prefix}\(");
        }

        public (List<string> result, bool invoked) Invoke(ITextReader reader, State state)
        {
            return base.Invoke(reader, state, -1);
        }
    }
}

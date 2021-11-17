using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class UndefineMacro : IMacro
    {
        private readonly DefineMacro defineMacro;

        public UndefineMacro(DefineMacro defineMacro)
        {
            this.defineMacro = defineMacro;
        }

        public string Prefix => "undefine";

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(txtReader, 0, state.Definitions);
            txtReader.Current.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (defineMacro.IsDefined(name))
            {
                state.Definitions.Remove(name);
                defineMacro.Remove(name);
            }
            return (new List<string>() { string.Empty }, true);
        }

        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, @$"^{Prefix}\b");
            }
            return Regex.IsMatch(txtReader.Current.Remainder, $@"\b{Prefix}\b");
        }
    }
}

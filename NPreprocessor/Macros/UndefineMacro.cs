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

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(reader.Current);
            reader.Consume(call.length);
            var args = call.args;
            var name = MacroString.Trim(args[0]);

            if (defineMacro.IsDefined(name))
            {
                defineMacro.Remove(name);
                return (new List<string>() { reader.Current }, true);
            }
            else
            {
                return (new List<string>() { reader.Current }, false);
            }
        }

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }
            return Regex.IsMatch(currentLine.Current, $@"\b{Prefix}\b");
        }
    }
}

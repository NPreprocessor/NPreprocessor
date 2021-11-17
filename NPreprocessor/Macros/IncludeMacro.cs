using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class IncludeMacro : IMacro
    {
        public IncludeMacro(DefineMacro defineMacro)
        {
            DefineMacro = defineMacro;
        }

        public string Prefix => "include";

        public static Func<string, string> Provider { get; set; } = (fileName) => File.ReadAllText(fileName);

        public DefineMacro DefineMacro { get; }

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(txtReader, 0, state.Definitions);
            txtReader.Current.Consume(call.length);
            var args = call.args;
            var fileNameExpression = args[0];

            var expressionTxtReader = new TextReader(fileNameExpression);
            expressionTxtReader.MoveNext();
            var results = DefineMacro.Invoke(expressionTxtReader, state);
            var fileName = MacroString.Trim(results.result[0]);
            string fileContent = Provider(fileName);
            return (new List<string>() { fileContent }, true);
        }

        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, $"^{Prefix}");
            }
            return Regex.IsMatch(txtReader.Current.Remainder, $@"\b{Prefix}\b");
        }
    }
}

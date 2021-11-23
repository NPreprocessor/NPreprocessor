using System;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor.Macros
{
    public class AritimeticMacroBase
    {
        private Dictionary<string, double> _lastResults = new Dictionary<string, double>();

        public AritimeticMacroBase(DefineMacro defineMacro)
        {
            DefineMacro = defineMacro;
        }

        public DefineMacro DefineMacro { get; }

        protected (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state, int modification)
        {
            var call = CallParser.GetInvocation(txtReader, 0, state.Definitions);
            var args = call.args;

            if (args.Length < 1)
            {
                throw new System.Exception("Invalid decr");
            }

            var expression = MacroString.Trim(args[0]);

            double result = 0;
            if (double.TryParse(expression, out var parseResult))
            {
                result = parseResult + modification;
            }
            else
            {
                var expressionTxtReader = new TextReader(expression, state.NewLineEnding);
                expressionTxtReader.MoveNext();
                var results = DefineMacro.Invoke(expressionTxtReader, state);

                if (results.invoked)
                {
                    var line = results.result.First();
                    if (double.TryParse(line, out var parseResult2))
                    {
                        result = parseResult2 + modification;
                    }
                    else
                    {
                        if (_lastResults.ContainsKey(expression))
                        {
                            result = _lastResults[expression] + modification;
                        }
                    }
                }
            }
            txtReader.Current.Consume(call.length);

            _lastResults[expression] = result;

            return (new List<string> { @result.ToString() }, true);
        }
    }
}
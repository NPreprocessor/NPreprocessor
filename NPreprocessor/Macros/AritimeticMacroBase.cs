using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class AritimeticMacroBase
    {
        public AritimeticMacroBase()
        {
        }

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
            if (double.TryParse(expression, out var exp1))
            {
                result = exp1 + modification;
            }
            else
            {
                if (state.Mappings.ContainsKey(expression) && double.TryParse(state.Mappings[expression], out var exp))
                {
                    result = exp + modification;
                }
            }
            txtReader.Current.Consume(call.length);

            return (new List<string> { @result.ToString() }, true);
        }
    }
}
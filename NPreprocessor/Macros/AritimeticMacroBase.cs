using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class AritimeticMacroBase
    {
        public AritimeticMacroBase()
        {
        }

        protected Task<List<TextBlock>> Invoke(ITextReader txtReader, State state, int modification)
        {
            int line = txtReader.Current.LineNumber;
            int column = txtReader.Current.ColumnNumber;
            
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
            txtReader.Current.Advance(call.length);

            return Task.FromResult(new List<TextBlock>
            {
                new TextBlock(@result.ToString())
                {
                    Line = line,
                    Column = column,
                    Finished = true

                }
            });
        }
    }
}
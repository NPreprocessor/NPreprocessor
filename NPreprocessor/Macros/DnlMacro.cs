using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Pattern => "dnl";

        public bool AreArgumentsRequired => false;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            reader.Current.Finish();
            
            if (state.NewLinePoints == 0)
            {
                state.NewLinePoints -= 1;
            }
            return Task.FromResult((new List<TextBlock>() { }, true));
        }
    }
}

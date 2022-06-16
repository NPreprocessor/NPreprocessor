using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class IncrMacro : AritimeticMacroBase, IMacro
    {
        public IncrMacro()
        {
        }

        public string Pattern => "incr";

        public bool AreArgumentsRequired => true;

        public int Priority { get; set; }

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            return base.Invoke(reader, state, 1);
        }
    }
}

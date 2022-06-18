using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DecrMacro : AritimeticMacroBase, IMacro
    {
        public DecrMacro() 
        {
        }

        public string Pattern => "decr";

        public bool AreArgumentsRequired => true;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            return base.Invoke(reader, state, -1);
        }
    }
}

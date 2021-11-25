using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class DecrMacro : AritimeticMacroBase, IMacro
    {
        public DecrMacro() 
        {
        }

        public string Pattern => "decr";

        public bool AreArgumentsRequired => true;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            return base.Invoke(reader, state, -1);
        }
    }
}

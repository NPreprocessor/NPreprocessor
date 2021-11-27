using System;
using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class IncrMacro : AritimeticMacroBase, IMacro
    {
        public IncrMacro()
        {
        }

        public string Pattern => "incr";

        public bool AreArgumentsRequired => true;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            return base.Invoke(reader, state, 1);
        }
    }
}

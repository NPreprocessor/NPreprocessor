using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Pattern => "dnl";

        public bool AreArgumentsRequired => false;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            reader.Current.Finish();
            
            if (state.NewLinePoints == 0)
            {
                state.NewLinePoints -= 1;
            }
            return (new List<TextBlock>() { }, true);
        }
    }
}

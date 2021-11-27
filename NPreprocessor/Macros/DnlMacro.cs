using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Pattern => "dnl";

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            reader.Current.Finish();
            state.MergePoints += 2;
            return (new List<string>() { string.Empty }, true);
        }
    }
}

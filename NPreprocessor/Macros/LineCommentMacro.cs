using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class LineCommentMacro : IMacro
    {
        public string Pattern => "//";

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            string comment = reader.Current.Remainder;
            reader.Current.Finish();

            return (new List<string>() { comment }, true);
        }
    }
}

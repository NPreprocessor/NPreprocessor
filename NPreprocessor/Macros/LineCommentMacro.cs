using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class LineCommentMacro : IMacro
    {
        public string Pattern => "//";

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            string comment = txtReader.Current.Remainder;
            txtReader.Current.Finish();

            return (new List<string>() { comment }, true);
        }
    }
}

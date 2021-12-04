using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class LineCommentMacro : IMacro
    {
        public string Pattern => "//";

        public bool AreArgumentsRequired => false;

        public bool IgnoreComment { get; set; } = false;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            string comment = reader.Current.Remainder;
            reader.Current.Finish();

            return (new List<TextBlock>() { IgnoreComment ? string.Empty : comment }, true);
        }
    }
}

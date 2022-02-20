using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class LineCommentMacro : IMacro
    {
        public string Pattern => "//";

        public bool AreArgumentsRequired => false;

        public bool IgnoreComment { get; set; } = false;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            string comment = reader.Current.Remainder;
            reader.Current.Finish(keapNewLine: true);


            return Task.FromResult((!IgnoreComment ?
                 new List<TextBlock>() {
                    new CommentTextBlock(comment)
                    {
                        Line = reader.LineNumber,
                        Position = reader.Current.CurrentAbsolutePosition,
                        Column = reader.Current.CurrentPosition
                    }} : new List<TextBlock>(), true));
        }
    }
}

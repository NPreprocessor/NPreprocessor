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
        
        public int Priority { get; set; }

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            int column = reader.Current.ColumnNumber;
            int line = reader.Current.LineNumber;

            string comment = reader.Current.Remainder;
            reader.Current.Finish(keepNewLine: true);


            return Task.FromResult((!IgnoreComment ?
                 new List<TextBlock>() {
                    new CommentBlock(comment)
                    {
                        Line = line,
                        Column = column
                    }} : new List<TextBlock>(), true));
        }
    }
}

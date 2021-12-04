using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class BlockCommentMacro : IMacro
    {
        public string Pattern => @"\/\*";

        public bool AreArgumentsRequired => false;

        public bool IgnoreComment { get; set; } = false;

        public (List<TextBlock> result, bool finished) Invoke(ITextReader reader, State state)
        {
            string candidate = reader.Current.Remainder;

            while (reader.Current?.Remainder != null && !candidate.Contains("*/"))
            {
                reader.MoveNext();

                candidate += reader.Current.Remainder;
            }

            int endPosition = candidate.IndexOf("*/");

            if (endPosition == -1)
            {
                throw new System.Exception("Cannot find ending of block commment");
            }

            var comment = candidate.Substring(0, endPosition + 2);
            int endPositionInCurrentLine = reader.Current.Remainder.IndexOf("*/");
            reader.Current.Consume(endPositionInCurrentLine + 2);

            if (IgnoreComment)
            {
                return (new List<TextBlock>() { }, true);
            }
            return (new List<TextBlock>() { comment }, true);
        }
    }
}

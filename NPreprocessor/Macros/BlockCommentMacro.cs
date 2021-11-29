using System;
using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public class BlockCommentMacro : IMacro
    {
        public string Pattern => @"\/\*";

        public bool AreArgumentsRequired => false;

        public bool IgnoreComment { get; set; } = false;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            string candidate = reader.Current.Remainder;

            while (reader.Current?.Remainder != null && !candidate.Contains("*/"))
            {
                reader.MoveNext();

                candidate += Environment.NewLine + reader.Current.Remainder;
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
                return (new List<string>() { string.Empty }, true);
            }
            return (new List<string>(comment.Split(Environment.NewLine)), true);
        }
    }
}

using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class BlockCommentMacro : IMacro
	{
		public string Pattern => @"\/\*";

		public bool AreArgumentsRequired => false;

		public bool IgnoreComment { get; set; } = false;
        
		public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
		{
			int column = reader.Current.ColumnNumber;
			int line = reader.Current.LineNumber;

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
			reader.Current.Advance(endPositionInCurrentLine + 2);

			if (IgnoreComment)
			{
				return Task.FromResult(new List<TextBlock>() { });
			}

			return Task.FromResult(
				new List<TextBlock>() { 
					new CommentBlock(comment)
					{
						Line = line,
						Column = column,
						Finished = true
					}});
		}
	}
}

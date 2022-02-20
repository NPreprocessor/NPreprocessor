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

		public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
		{
			int position = reader.Current.CurrentAbsolutePosition;
			int column = reader.Current.CurrentPosition;

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
				return Task.FromResult((new List<TextBlock>() { }, true));
			}

			return Task.FromResult((
				new List<TextBlock>() { 
					new CommentTextBlock(comment)
					{
						Line = reader.LineNumber,
						Position = position,
						Column = column,
					}}, true));
		}
	}
}

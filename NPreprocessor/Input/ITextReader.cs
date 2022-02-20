using System.Collections.Generic;

namespace NPreprocessor.Input
{
    public interface ITextReader : IEnumerator<ITextLineReader>
    {
        int LineNumber { get; }

        string LineContinuationCharacters { get; }

        string NewLineEnding { get; }

        bool AppendNext();
    }
}

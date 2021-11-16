using System.Collections.Generic;

namespace NPreprocessor
{
    public interface ITextReader : IEnumerator<ILineReader>
    {
        int LineNumber { get; }

        string LineContinuationCharacters { get; }

        bool AppendNext();
    }
}

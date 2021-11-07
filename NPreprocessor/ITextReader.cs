using System.Collections.Generic;

namespace NPreprocessor
{
    public interface ITextReader : IEnumerator<string>
    {
        int LineNumber { get; }

        string LineContinuationCharacters { get; }
    }
}

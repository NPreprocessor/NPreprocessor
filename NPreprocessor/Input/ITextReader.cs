using System.Collections.Generic;

namespace NPreprocessor.Input
{
    public interface ITextReader
    {
        LogicalLineReader Current { get; }

        bool MoveNext();

        void ReplaceCurrentLine(List<LogicalLine> logicalLines);
    }
}
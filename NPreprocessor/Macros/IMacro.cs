using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public interface IMacro
    {
        bool CanBeUsed(ILineReader currentLine, bool atStart);

        (List<string> result, bool invoked) Invoke(ILineReader currentLineReader, ITextReader reader, State state);
    }
}
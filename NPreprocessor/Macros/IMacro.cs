using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public interface IMacro
    {
        bool CanBeUsed(ITextReader reader, bool atStart);

        (List<string> result, bool invoked) Invoke(ITextReader reader, State state);
    }
}
using System.Collections.Generic;

namespace NPreprocessor.Macros
{
    public interface IMacro
    {
        string Pattern { get; }

        bool AreArgumentsRequired { get; }

        (List<string> result, bool finished) Invoke(ITextReader reader, State state);
    }

    public interface IDynamicMacro : IMacro
    {
        bool CanBeInvoked(ITextReader reader, State state, out int index);
    }
}
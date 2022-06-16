using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public interface IMacro
    {
        string Pattern { get; }

        bool AreArgumentsRequired { get; }

        int Priority { get; set; }

        Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state);
    }

    public interface IDynamicMacro : IMacro
    {
        bool CanBeInvoked(ITextReader reader, State state, out int index);
    }
}
using NPreprocessor.Input;
using NPreprocessor.Macros;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor
{
    public interface IMacroResolver
    {
        List<IMacro> Macros { get; }

        Task<MacroResolverResult> Resolve(ITextReader txtReader, State state = null);
    }
}

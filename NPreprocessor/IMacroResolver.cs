using NPreprocessor.Macros;
using System.Collections.Generic;

namespace NPreprocessor
{
    public interface IMacroResolver
    {
        List<IMacro> Macros { get; }

        List<string> Resolve(ITextReader txtReader, State state = null);
    }
}

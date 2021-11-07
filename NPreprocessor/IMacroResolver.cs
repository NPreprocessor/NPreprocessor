using NPreprocessor.Macros;
using System.Collections.Generic;

namespace NPreprocessor
{
    public interface IMacroResolver
    {
        List<IMacro> Macros { get; }
        List<string> Do(ITextReader lineReader, State state);
        List<string> Do(ITextReader txtReader);
        List<string> DoAll(ITextReader txtReader);
        List<string> DoAll(ITextReader txtReader, State state);
    }
}

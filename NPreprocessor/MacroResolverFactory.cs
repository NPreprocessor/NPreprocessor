using NPreprocessor.Macros;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class MacroResolverFactory
    {
        public static MacroResolver CreateDefault(bool ignoreComments, string newLineEnding)
        {
            var macros = new List<IMacro>();

            macros.Add(new BlockCommentMacro() { IgnoreComment = ignoreComments });
            macros.Add(new LineCommentMacro() { IgnoreComment = ignoreComments });
            macros.Add(new UndefineMacro());
            macros.Add(new DefineMacro());
            macros.Add(new DefineResolveMacro());
            macros.Add(new IfDefMacro());
            macros.Add(new IfNDefMacro());
            macros.Add(new IncludeMacro());
            macros.Add(new DnlMacro());
            macros.Add(new IncrMacro());
            macros.Add(new DecrMacro());
            macros.Add(new RegexMacro());
            macros.Add(new RegexResolveMacro());

            var resolver = new MacroResolver(macros);
            resolver.Macros.Add(new IfMacro(resolver));
            return resolver;
        }
    }
}

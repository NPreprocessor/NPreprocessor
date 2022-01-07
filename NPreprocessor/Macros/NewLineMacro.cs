using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class NewLineMacro : IMacro
    {
        public NewLineMacro(string pattern)
        {
            Length = pattern.Length;
            Pattern = Regex.Escape(pattern);
        }

        public int Length { get; }

        public string Pattern { get; }

        public bool AreArgumentsRequired => false;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            state.NewLinePoints++;
            reader.Current.Consume(Length);

            if (state.CreateNewLine)
            {
                state.NewLinePoints--;
                return Task.FromResult((new List<TextBlock>() { state.NewLineEnding }, true));
            }
            else
            {
                return Task.FromResult((new List<TextBlock>() { }, true));
            }
        }
    }
}

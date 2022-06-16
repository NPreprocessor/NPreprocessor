using NPreprocessor.Input;
using NPreprocessor.Output;
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

        public int Priority { get; set; }

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            int line = reader.Current.LineNumber;
            int column = reader.Current.ColumnNumber;

            state.NewLinePoints++;
            reader.Current.Advance(Length);

            if (state.CreateNewLine)
            {
                state.NewLinePoints--;
                return Task.FromResult((new List<TextBlock>() { new TextBlock(state.NewLineEnding) { Line = line, Column = column }}, true));
            }
            else
            {
                return Task.FromResult((new List<TextBlock>() { }, true));
            }
        }
    }
}

using NPreprocessor.Input;
using NPreprocessor.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class IncludeMacro : IMacro
    {
        public IncludeMacro()
        {
        }

        public string Pattern => "include";

        public Func<string, Task<string>> Provider { get; set; } = (fileName) => File.ReadAllTextAsync(fileName);

        public DefineMacro DefineMacro { get; }

        public bool AreArgumentsRequired => true;

        public async Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            int position = reader.Current.CurrentAbsolutePosition;
            int column = reader.Current.CurrentPosition;

            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Advance(call.length);
            var args = call.args;
            var fileNameExpression = args[0];
            var fileName = MacroString.Trim(fileNameExpression);
            string fileContent = await Provider(fileName);

            return (new List<TextBlock>() { new TextBlock(fileContent) { Column = column, Position = position, Line = reader.LineNumber }}, false);
        }
    }
}

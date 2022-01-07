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
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Consume(call.length);
            var args = call.args;
            var fileNameExpression = args[0];
            var fileName = MacroString.Trim(fileNameExpression);
            string fileContent = await Provider(fileName);

            return (new List<TextBlock>() { fileContent }, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace NPreprocessor.Macros
{
    public class IncludeMacro : IMacro
    {
        public IncludeMacro()
        {
        }

        public string Pattern => "include";

        public static Func<string, string> Provider { get; set; } = (fileName) => File.ReadAllText(fileName);

        public DefineMacro DefineMacro { get; }

        public bool AreArgumentsRequired => true;

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Consume(call.length);
            var args = call.args;
            var fileNameExpression = args[0];
            var fileName = MacroString.Trim(fileNameExpression);
            string fileContent = Provider(fileName);
            return (new List<string>() { fileContent }, false);
        }
    }
}

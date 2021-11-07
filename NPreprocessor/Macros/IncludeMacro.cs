using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class IncludeMacro : IMacro
    {
        public string Prefix => "include";

        public static Func<string, string> Provider { get; set; } = (fileName) => File.ReadAllText(fileName);

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var call = CallParser.GetInvocation(reader.Current);
            reader.Consume(call.length);
            var args = call.args;
            var fileName = MacroString.Trim(args[0]);
            string fileContent = Provider(fileName);
            return (new List<string>() { fileContent }, true);
        }

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }
            return Regex.IsMatch(currentLine.Current, $@"\b{Prefix}\b");
        }
    }
}

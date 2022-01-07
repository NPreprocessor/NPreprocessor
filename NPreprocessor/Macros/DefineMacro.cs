using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DefineMacro : IMacro
    {
        public DefineMacro()
        {
        }

        public string Pattern { get; } = "define";

        public bool AreArgumentsRequired => true;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);

            if (call.name == null)
            {
                return Task.FromResult<(List<TextBlock> result, bool finished)>((null, false));
            }

            var args = call.args;

            if (args.Length < 1)
            {
                throw new System.Exception("Invalid def");
            }
            reader.Current.Consume(call.length);

            var name = MacroString.Trim(args[0]);

            if (args.Length >= 2)
            {
                var value = args[1];
                state.Mappings[name] = MacroString.Trim(value);
                if (Regex.IsMatch(value, @"\$\d+") || value.Contains("\"$\""))
                {
                    state.MappingsParameters[name] = true;
                }
                else
                {
                    state.MappingsParameters[name] = false;
                }
            }
            else
            {
                state.Mappings[name] = string.Empty;
            }

            state.Definitions.Add(name);

            return Task.FromResult((new List<TextBlock> { string.Empty }, true));
        }
    }
}

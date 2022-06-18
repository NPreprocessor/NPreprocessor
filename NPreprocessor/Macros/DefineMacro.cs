using NPreprocessor.Input;
using NPreprocessor.Output;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DefineMacro : IMacro
    {
        private readonly Regex _digitMacro;

        public DefineMacro()
        {
            _digitMacro = new Regex(@"\$\d+");
        }

        public string Pattern { get; } = "define";

        public bool AreArgumentsRequired => true;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);

            if (call.name == null)
            {
                return Task.FromResult<List<TextBlock>>(null);
            }

            var args = call.args;

            if (args.Length < 1)
            {
                throw new System.Exception("Invalid def");
            }
            reader.Current.Advance(call.length);

            var name = MacroString.Trim(args[0]);

            if (args.Length >= 2)
            {
                var value = args[1];
                state.Mappings[name] = MacroString.Trim(value);
                if (_digitMacro.IsMatch(value) || value.Contains("\"$\""))
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

            return Task.FromResult(new List<TextBlock>());
        }
    }
}

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedDefineMacro : IMacro
    {
        private Regex _method;
        private Regex _const;

        public ExpandedDefineMacro(string prefix)
        {
            _method = new Regex(@$"^{prefix}\s+([\$_\w]+)\((.*?)\)\s*(.+)");
            _const = new Regex(@$"^{prefix}\s+([\\@\$\w]+)\s*(.*)");

            Pattern = prefix;
        }

        public string Pattern { get; set; }

        public bool AreArgumentsRequired => false;

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            var line = txtReader.Current.Remainder;
            txtReader.Current.Finish();
            var methodMatch = _method.Match(line);

            if (methodMatch.Success)
            {
                var name = methodMatch.Groups[1].Value;
                var args = methodMatch.Groups[2].Value;
                var value = methodMatch.Groups[3].Value;
                var argsSplited = args.Split(',');
                for (var i = 1; i <= argsSplited.Length; i++)
                {
                    var arg = argsSplited[i-1].Trim();
                    value = value.Replace($"{arg}", $"${i}");
                }

                return (new List<string>() { $"define(`{state.DefinitionPrefix}{name}', `{MacroString.Escape(value)}')" }, false);
            }
            else
            {
                var constMatch = _const.Match(line);
                var name = constMatch.Groups[1].Value;
                var value = constMatch.Groups[2].Value;
                return (new List<string>() { $"define(`{state.DefinitionPrefix}{name}', `{MacroString.Escape(value)}')" }, false);
            }
        }
    }
}

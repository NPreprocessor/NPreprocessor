using NPreprocessor;
using NPreprocessor.Macros;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedDefineMacro : IMacro
    {
        private readonly string defPrefix;
        private Regex _method;
        private Regex _const;

        public ExpandedDefineMacro(string prefix, string defPrefix = "")
        {
            _method = new Regex(@$"^{prefix}\s+([\$_\w]+)\((.*?)\)\s*(.+)");
            _const = new Regex(@$"^{prefix}\s+([\\@\$\w]+)\s*(.*)");

            Prefix = prefix;
            this.defPrefix = defPrefix;
        }

        public string Prefix { get; set; }

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var line = reader.Current;
            reader.Finish();
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

                return (new List<string>() { $"define(`{defPrefix}{name}', `{MacroString.Escape(value)}')" }, true);
            }
            else
            {
                var constMatch = _const.Match(line);
                var name = constMatch.Groups[1].Value;
                var value = constMatch.Groups[2].Value;
                return (new List<string>() { $"define(`{defPrefix}{name}', `{MacroString.Escape(value)}')" }, true);
            }
        }

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }

            return Regex.IsMatch(currentLine.Current, $@"{Prefix}");
        }
    }
}

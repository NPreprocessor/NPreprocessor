using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedDefineMacro : IMacro
    {
        private readonly Regex _method;
        private readonly Regex _const;

        public ExpandedDefineMacro(string prefix)
        {
            _method = new Regex(@$"^{prefix}\s+([\$_\w]+)\((.*?)\)\s*(.+)");
            _const = new Regex(@$"^{prefix}\s+([\\@\$\w]+)\s*(.*)");

            Pattern = prefix;
        }

        public string Pattern { get; set; }

        public bool AreArgumentsRequired => false;

        public bool KeepAsComments { get; set; } = false;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader txtReader, State state)
        {
            int lineNumber = txtReader.Current.LineNumber;
            int columnNumber = txtReader.Current.ColumnNumber;
            string line = GetLine(txtReader.Current.Remainder);
            string lineRaw = txtReader.Current.RemainderRaw;

            txtReader.Current.Finish(keepNewLine: false);

            var methodMatch = _method.Match(line);

            if (methodMatch.Success)
            {
                var name = methodMatch.Groups[1].Value;
                var args = methodMatch.Groups[2].Value;
                var value = methodMatch.Groups[3].Value.Trim();
                var argsSplited = args.Split(',');
                for (var i = 1; i <= argsSplited.Length; i++)
                {
                    var arg = argsSplited[i - 1].Trim();
                    value = Regex.Replace(value, @$"(?<=\b|\W|\s){arg}(?=\b|\W|\s)", $"${i}");
                }

                var result = new List<TextBlock>() { new TextBlock($"define(`{state.DefinitionPrefix}{name}', `{MacroString.Escape(value)}')") { Column = columnNumber, Line = lineNumber } };

                if (KeepAsComments)
                {
                    result.Insert(0, new MacroCommentBlock(lineRaw)
                    {
                        Column = columnNumber,
                        Line = lineNumber,
                        Finished = true
                    });
                }

                return Task.FromResult(result);
            }
            else
            {
                var constMatch = _const.Match(line);
                if (constMatch.Success)
                {
                    var name = constMatch.Groups[1].Value;
                    var value = constMatch.Groups[2].Value.Trim();
                    var result = new List<TextBlock>() { new TextBlock($"define(`{state.DefinitionPrefix}{name}', `{MacroString.Escape(value)}')") { Column = columnNumber, Line = lineNumber } };

                    if (KeepAsComments)
                    {
                        result.Insert(0, new MacroCommentBlock(lineRaw)
                        {
                            Column = columnNumber,
                            Line = lineNumber,
                            Finished = true
                        });
                    }
                    return Task.FromResult(result);
                }
                else
                {
                    return Task.FromResult(new List<TextBlock>() { new TextBlock(line) { Column = columnNumber, Line = lineNumber, Finished = true } });
                }
            }
        }
        private static string GetLine(string fullLine)
        {
            var commentIndex = fullLine.IndexOf("//");

            if (commentIndex != -1)
            {
                fullLine = fullLine.Substring(0, commentIndex);
            }

            return fullLine;
        }
    }
}

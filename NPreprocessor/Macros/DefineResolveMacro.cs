using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DefineResolveMacro : IDynamicMacro
    {
        private readonly Dictionary<string, Regex> _cache = new Dictionary<string, Regex>();
        private readonly Dictionary<string, Regex> _escaped = new Dictionary<string, Regex>();

        public DefineResolveMacro()
        {
        }

        public string Pattern => null;

        public bool AreArgumentsRequired => false;

        public int Priority { get; set; }

        public bool CanBeInvoked(ITextReader reader, State state, out int index)
        {
            string result = reader.Current.Remainder;
            var match = state.Mappings.Keys
                .Select(key => GetRegex(key).Match(result))
                .Where(s => s.Success && !IsInsideString(result, s.Groups[1].Index, state))
                .OrderBy(i => i.Groups[1].Index)
                .FirstOrDefault();

            if (match != null)
            {
                index = match.Groups[1].Index;
                return true;
            }

            index = -1;
            return false;
        }

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            bool resolved = false;
            int column = reader.Current.ColumnNumber;
            int line = reader.Current.LineNumber;
            string initial = reader.Current.Remainder;
            string result = reader.Current.Remainder;

            var item = state.Mappings.Keys
                .Select(key => (key, GetRegex(key).Match(result)))
                .Where(match => match.Item2.Success && !IsInsideString(result, match.Item2.Groups[1].Index, state))
                .OrderBy(i => i.Item2.Groups[1].Index)
                .FirstOrDefault();

            if (item != default)
            {
                var key = item.key;
                var match = item.Item2;
                int index = match.Groups[1].Index;

                var replacement = state.Mappings[key];
                if (state.MappingsParameters.ContainsKey(key) && state.MappingsParameters[key])
                {
                    var remainder = result.Substring(index);

                    var call = CallParser.GetInvocation(reader, index, state.Definitions);

                    replacement = replacement.Replace($"$0", key);

                    int i = 1;

                    if (call.args != null)
                    {
                        if (replacement.Contains("\"$\""))
                        {
                            replacement = replacement.Replace("\"$\"", "$" + MacroString.Trim(call.args[0]));
                        }

                        foreach (var arg in call.args)
                        {
                            var argR = MacroString.Trim(call.args[i - 1]);
                            replacement = Regex.Replace(replacement, @$"\${i}(?=\b|\s|\W)", argR);
                            i++;
                        }
                    }

                    if (call.length > 0)
                    {
                        var callString = remainder.Substring(0, call.length);
                        var regex = GetEscapedRegex(callString);
                        result = regex.Replace(result, replacement, 1);
                    }
                    else
                    {
                        var regex = GetEscapedRegex(key);
                        result = regex.Replace(result, replacement, 1);
                    }
                }
                else
                {
                    var regex = GetEscapedRegex(key);
                    result = regex.Replace(result, replacement, 1);
                }

                resolved = true;
            }

            if (resolved)
            {
                reader.Current.Advance(initial.Length);
            }

            return Task.FromResult((
                new List<TextBlock>()
                {
                    new TextBlock(result)
                    {
                        Column = column,
                        Line = line
                    }
                }, !resolved));
        }
        

        private Regex GetEscapedRegex(string key)
        {
            if (!_escaped.ContainsKey(key))
            {
                _escaped[key] = new Regex(Regex.Escape(key));
            }

            return _escaped[key];
        }

        private Regex GetRegex(string key)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = new Regex(@"(?<=^|\b|\s|\W)(" + Regex.Escape(key) + @")(?=\b|\s|\W)");
            }

            return _cache[key];
        }

        private static bool IsInsideString(string result, int index, State state)
        {
            var lastStartPos = result.LastIndexOf(index, '`');

            if (lastStartPos == -1) return false;

            if (state.Mappings.Keys.Any(d => result.StartsWith(lastStartPos, d)))
            {
                return false;
            }

            return true;
        }

    }
}

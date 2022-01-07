using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DefineResolveMacro : IDynamicMacro
    {
        public DefineResolveMacro()
        {
        }

        public string Pattern => null;

        public bool AreArgumentsRequired => false;

        public bool CanBeInvoked(ITextReader reader, State state, out int index)
        {
            string result = reader.Current.Remainder;
            var match = state.Mappings.Keys
                .Select(key => Regex.Match(result, GetRegex(key)))
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
            string initial = reader.Current.Remainder;
            string result = reader.Current.Remainder;

            var item = state.Mappings.Keys
                .Select(key => (key, Regex.Match(result, GetRegex(key))))
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
                        var regex = new Regex(Regex.Escape(callString));
                        result = regex.Replace(result, replacement, 1);
                    }
                    else
                    {
                        var regex = new Regex(Regex.Escape(key));
                        result = regex.Replace(result, replacement, 1);
                    }
                }
                else
                {
                    var regex = new Regex(Regex.Escape(key));
                    result = regex.Replace(result, replacement, 1);
                }

                resolved = true;
            }

            if (resolved)
            {
                reader.Current.Consume(initial.Length);
            }

            return Task.FromResult((new List<TextBlock>() { result }, !resolved));
        }

        private static string GetRegex(string key)
        {
            return @"(?<=^|\b|\s|\W)(" + Regex.Escape(key) + @")(?=\b|\s|\W)";
        }

        private static bool IsInsideString(string result, int index, State state)
        {
            var lastStartPos = result.Substring(0, index).LastIndexOf('`');

            if (lastStartPos == -1) return false;

            if (state.Mappings.Keys.Any(d => result.Substring(lastStartPos).StartsWith(d)))
            {
                return false;
            }

            return true;
        }
    }
}

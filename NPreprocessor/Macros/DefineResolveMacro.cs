﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros
{
    public class DefineResolveMacro : IDynamicMacro
    {
        public DefineResolveMacro()
        {
        }

        public string Pattern => null;

        public bool AreArgumentsRequired => false;

        public bool CanBeInvoked(ITextReader txtReader, State state)
        {
            string result = txtReader.Current.Remainder;
            return state.Mappings.Keys
                .Select(key => Regex.Match(result, GetRegex(key)))
                .Where(s => s.Success)
                .Where(s => !IsInsideString(result, s.Groups[1].Index, state))
                .Any();
        }

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            bool resolved = false;
            string initial = txtReader.Current.Remainder;
            string result = txtReader.Current.Remainder;

            var item = state.Mappings.Keys
                .Select(key => (key, Regex.Match(result, GetRegex(key))))
                .Where(match => match.Item2.Success && !IsInsideString(result, match.Item2.Groups[1].Index, state))
                .OrderBy(i => i.Item2.Index).ToList()
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

                    var call = CallParser.GetInvocation(txtReader, index, state.Definitions);

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
                            replacement = replacement.Replace($"${i}", argR);
                            i++;
                        }
                    }
                    if (call.length > 0)
                    {
                        var callString = remainder.Substring(0, call.length);
                        result = result.Replace(callString, replacement);
                    }
                    else
                    {
                        result = result.Replace(key, replacement);
                    }
                }
                else
                {
                    result = result.Replace(key, replacement);
                }

                resolved = true;
            }

            if (resolved)
            {
                txtReader.Current.Consume(initial.Length);
            }

            return (new List<string>(result.Split(Environment.NewLine)), !resolved);
        }

        private static string GetRegex(string key)
        {
            return @"(?:^|\b|\s|\W)(" + Regex.Escape(key) + @")(?:\b|\s|\W)";
        }

        private bool IsInsideString(string result, int index, State state)
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
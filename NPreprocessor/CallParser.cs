using NPreprocessor.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor
{
    public static class CallParser
    {
        private static Regex _macroCallPrefix = new Regex(@"([`\$\w]+)\(", RegexOptions.Singleline);

        public static (string name, string[] args, int length) GetInvocation(ITextReader reader, int startIndex = 0, HashSet<string> defs = null)
        {
            var match = GetMatch(reader.Current.Remainder, startIndex, defs);

            if (match.success)
            {
                var name = match.name;
                var argsWithBrackets = match.args;
                var args = SplitArguments(argsWithBrackets, defs);

                return (name, args, name.Length + 2 + argsWithBrackets.Length);
            }
            else
            {
                if (reader.AppendNext())
                {
                    return GetInvocation(reader, startIndex);
                }
            }

            return (null, null, 0);
        }

        private static (bool success, string name, string args) GetMatch(string remainder, int startIndex, HashSet<string> defs)
        {
            var match = _macroCallPrefix.Match(remainder, startIndex);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                int counter = 0;
                bool insideString = false;
                bool insideQuotes = false;

                int i = 0;
                int start = match.Index + name.Length;
                for (i = start; i < remainder.Length; i++)
                {
                    if (!insideQuotes && remainder[i] == '`' && (defs == null || defs.All(d => !remainder.StartsWith(i, d))))
                    {
                        insideString = true;
                    }

                    if (i < remainder.Length - 1 && remainder.Substring(i, 2) == @"\'")
                    {
                        i += 2;
                        continue;
                    }

                    if (i < remainder.Length - 1 && remainder.Substring(i, 2) == @"\""")
                    {
                        i += 2;
                        continue;
                    }

                    if (remainder[i] == '\'')
                    {
                        insideString = false;
                    }

                    if (remainder[i] == '"')
                    {
                        insideQuotes = !insideQuotes;
                    }

                    if (remainder[i] == '(' && !insideString)
                    {
                        counter++;
                    }

                    if (remainder[i] == ')' && !insideString)
                    {
                        counter--;
                    }

                    if (counter == 0)
                    {
                        break;
                    }
                }

                if (counter == 0)
                {
                    var args = remainder.Substring(start + 1, i - start - 1);
                    return (true, name, args);
                }

                
            }
            return (false,null, null);
        }

        private static string[] SplitArguments(string args, HashSet<string> defs)
        {
            var list = new List<string>();
            var positions = new List<int>();

            int counter = 0;
            bool insideString = false;
            bool insideQuotes = false;
            for (var i = 0; i < args.Length; i++)
            {
                if (counter == 0 && args[i] == ',' && !insideString && !insideQuotes)
                {
                    positions.Add(i);
                }

                if (!insideQuotes && args[i] == '`' && (defs == null || defs.All(d => !args.StartsWith(i, d))))
                {
                    insideString = true;
                }

                if (i < args.Length - 1 && args.Substring(i, 2) == @"\'")
                {
                    i += 2;
                    continue;
                }

                if (i < args.Length - 1 && args.Substring(i, 2) == @"\""")
                {
                    i += 2;
                    continue;
                }

                if (args[i] == '\'')
                {
                    insideString = false;
                }

                if (args[i] == '"')
                {
                    insideQuotes = !insideQuotes;
                }

                if (args[i] == '(')
                {
                    counter++;
                }

                if (args[i] == ')')
                {
                    counter--;
                }
            }

            if (positions.Count == 0)
            {
                return new[] { args };
            }

            int pos = 0;
            for (var j = 0; j < positions.Count; j++)
            {
                var substring = args.Substring(pos, positions[j] - pos);
                list.Add(substring);
                pos += substring.Length + 1;
            }

            var last = args.Substring(pos);
            list.Add(last);

            return list.ToArray();
        }
    }
}

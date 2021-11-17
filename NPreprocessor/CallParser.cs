using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor
{
    public class CallParser
    {
        private static Regex _macroCallPrefix = new Regex(@"^([`\$\w]+)\(", RegexOptions.Singleline);

        public static (string name, string[] args, int length) GetInvocation(ITextReader reader, int startIndex = 0)
        {
            var match = GetMatch(reader.Current.Remainder, startIndex);

            if (match.success)
            {
                var name = match.name;
                var argsWithBrackets = match.args;
                var args = SplitArguments(argsWithBrackets);

                return (name, args, name.Length + 2 + argsWithBrackets.Length);
            }
            else
            {
                if (_macroCallPrefix.IsMatch(reader.Current.Remainder, startIndex))
                {
                    if (reader.AppendNext())
                    {
                        return GetInvocation(reader, startIndex);
                    }
                }
            }

            return (null, null, 0);
        }

        private static (bool success, string name, string args) GetMatch(string remainder, int startIndex)
        {
            var match = _macroCallPrefix.Match(remainder, startIndex);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                int counter = 0;
                bool insideString = false;
                int i = 0;
                int start = match.Length - 1;
                for (i = match.Length - 1; i < remainder.Length; i++)
                {
                    if (remainder[i] == '`')
                    {
                        insideString = true;
                    }

                    if (remainder[i] == '\'')
                    {
                        insideString = false;
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

        private static string[] SplitArguments(string args)
        {
            var list = new List<string>();
            var positions = new List<int>();

            int counter = 0;
            for (var i = 0; i < args.Length; i++)
            {
                if (counter == 0 && args[i] == ',')
                {
                    positions.Add(i);
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

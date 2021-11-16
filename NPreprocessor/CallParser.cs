using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor
{
    public class CallParser
    {
        private static Regex _invocation = new Regex(@"^([`\$\w]+)\(((?:[^\(\)])|(?<something>\()|(?<-something>\)))*?(?(something)(?!))\)", RegexOptions.Singleline);
        private static Regex _partial = new Regex(@"^([`\$\w]+)\(", RegexOptions.Singleline);

        public static (string name, string[] args, int length) GetInvocation(ITextReader reader, int startIndex = 0)
        {
            var matches = _invocation.Matches(reader.Current.Remainder, startIndex);

            if (matches.Count == 1)
            {
                var value = matches[0].Value;
                var name = matches[0].Groups[1].Value;
                var argsWithBrackets = value.Substring(name.Length);
                var argsTogether = argsWithBrackets.Substring(1, argsWithBrackets.LastIndexOf(')') - 1);
                var args = SplitArguments(argsTogether);

                return (name, args, name.Length + 2 + argsTogether.Length);
            }
            else
            {
                if (_partial.IsMatch(reader.Current.Remainder, startIndex))
                {
                    if (reader.AppendNext())
                    {
                        return GetInvocation(reader, startIndex);
                    }
                }
            }

            return (null, null, 0);
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

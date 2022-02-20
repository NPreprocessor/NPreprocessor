using NPreprocessor.Input;
using NPreprocessor.Output;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class RegexActionMacro : IMacro
    {
        public RegexActionMacro(string pattern, Action<Match> action = null, bool ignore = false)
        {
            Pattern = pattern;
            Action = action;
            Ignore = ignore;
        }

        public string Pattern { get; }

        public Action<Match> Action { get; }

        public bool Ignore { get; }

        public bool AreArgumentsRequired => false;

        public Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            string currentReminder = reader.Current.Remainder;
            var blocks = new List<TextBlock>();

            var match = Regex.Match(currentReminder, Pattern);

            if (match.Success)
            {
                Action?.Invoke(match);
                reader.Current.Advance(match.Length);
            }
            else
            {
                throw new InvalidOperationException("That shouldn't happen");
            }

            if (!Ignore)
            {
                blocks.Add(new TextBlock(currentReminder));
            }

            return Task.FromResult((blocks, true));
        }
    }
}

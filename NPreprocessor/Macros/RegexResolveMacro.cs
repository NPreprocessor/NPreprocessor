using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class RegexResolveMacro : IDynamicMacro
    {
        public RegexResolveMacro()
        {
        }

        public string Pattern => null;

        public bool AreArgumentsRequired => false;

        public int Priority { get; set; }

        public bool CanBeInvoked(ITextReader reader, State state, out int index)
        {
            string result = reader.Current.Remainder;
            var match = state.Regexes.Keys
                .Select(key => Regex.Match(result, key))
                .Where(s => s.Success)
                .OrderBy(i => i.Index)
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
            int column = reader.Current.ColumnNumber;
            int line = reader.Current.LineNumber;

            string result = reader.Current.Remainder;

            var item = state.Regexes.Keys
                .Select(key => (key, Regex.Match(result, key)))
                .Where(match => match.Item2.Success)
                .OrderBy(i => i.Item2.Index)
                .FirstOrDefault();

            if (item != default)
            {
                var key = item.key;
                var match = item.Item2;
                int index = match.Index;

                var replacement = state.Regexes[key];

                var regex = new Regex(key);

                var replacementUpdated = regex.Replace(result.Substring(match.Index, match.Length), replacement, 1, index);

                reader.Current.Advance(match.Value.Length);

                return Task.FromResult((new List<TextBlock> { new TextBlock(replacementUpdated) { Column = column, Line = line } }, false));
            }
            else
            {
                return Task.FromResult((new List<TextBlock> { new TextBlock(result) { Column = column, Line = line } }, true));
            }
        }
    }
}

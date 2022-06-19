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
            int diff = reader.Current.ColumnNumber;

            var match = state.Regexes.Keys
                .Select(key => Regex.Match(result, key))
                .Where(s => s.Success)
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

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            int column = reader.Current.ColumnNumber;
            int line = reader.Current.LineNumber;
            int diff = reader.Current.ColumnNumber;

            string result = reader.Current.RemainderWithoutNewLine;

            var item = state.Regexes.Keys
                .Select(key => (key, Regex.Match(result, key)))
                .Where(match => match.Item2.Success)
                .OrderBy(i => i.Item2.Groups[1].Index)
                .FirstOrDefault();

            if (item != default)
            {
                var key = item.key;
                var match = item.Item2;
                int index = match.Groups[1].Index;

                var replacement = state.Regexes[key];

                var regex = new Regex(key);

                var replacementUpdated = regex.Replace(result.Substring(index, match.Length), replacement, 1, 0);

                reader.Current.Advance(match.Value.Length);

                return Task.FromResult(new List<TextBlock> { new TextBlock(replacementUpdated) { Column = column, Line = line, Finished = true } });
            }
            else
            {
                throw new System.Exception("That should not happen");
            }
        }
    }
}

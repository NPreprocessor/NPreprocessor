using NPreprocessor.Input;
using NPreprocessor.Macros;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor
{
    public class MacroResolver : IMacroResolver
    {
        private readonly List<IMacro> _macros = new List<IMacro>();
        private readonly Dictionary<IMacro, Regex> _cache = new Dictionary<IMacro, Regex>();
        private readonly Regex _stringPattern = new Regex(@"""((?:\\.|[^\\""])*)""");
        public MacroResolver(List<IMacro> macros)
        {
            _macros = macros;           
        }

        public List<IMacro> Macros { get => _macros; }

        public async Task<MacroResolverResult> Resolve(ITextReader txtReader, State state = null)
        {
            if (state == null)
            {
                state = new State();
            }

            if (state.CurrentLineDisabledRanges == null)
            {
                state.CurrentLineDisabledRanges = new List<(int start, int end)>();
            }

            var result = new List<TextBlock>();
            while (true)
            {
                if (!txtReader.MoveNext())
                {
                    return new MacroResolverResult(result, state.NewLineEnding);
                }

                result.AddRange(await ProcessCurrentLine(txtReader, state));
            }
        }

        private async Task<List<TextBlock>> ProcessCurrentLine(ITextReader txtReader, State state)
        {
            var result = new List<TextBlock>();

            while (true)
            {
                var txt = txtReader.Current?.Remainder;

                if (txt == null)
                {
                    break;
                }

                var disabledRegions = _stringPattern.Matches(txtReader.Current.Remainder).Select(s => (s.Index, s.Index + s.Length - 1)).ToList();

                if (state.CurrentLineDisabledRanges != null)
                {
                    disabledRegions.AddRange(state.CurrentLineDisabledRanges);
                }

                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state, disabledRegions);

                if (macroToCall != default)
                {
                    string skipped = macroToCall.position > 0 ? txtReader.Current.Remainder.Substring(0, macroToCall.position) : "";
                    txtReader.Current.Advance(macroToCall.position);

                    var macroResult = await macroToCall.macro.Invoke(txtReader, state);

                    var macroResultLenght = macroResult.Sum(b => b.Value.Length);

                    string remainder = txtReader.Current?.Remainder ?? "";
                    state.CurrentLineDisabledRanges.AddRange(macroResult.Where(r => r.Finished).Select(skipped => (skipped.Column, skipped.Column + skipped.Value.Length - 1)));

                    string macroResultTxt = string.Join(string.Empty, macroResult.Select(b => b.Value));
                    string newLine = skipped + macroResultTxt + remainder;

                    var cascadeTextReader = new TextReader(newLine, state.NewLineEnding);
                    txtReader.ReplaceCurrentLine(cascadeTextReader.LogicalLines);
                }
                else
                {
                    if (txtReader.Current.Remainder != "")
                    {
                        result.Add(new TextBlock(txtReader.Current.RemainderRaw)
                        {
                            Column = txtReader.Current.ColumnNumber,
                            Line = txtReader.Current.LineNumber
                        });
                    }

                    state.CurrentLineDisabledRanges.Clear();
                    break;
                }
            }
            return result;
        }

        private (IMacro macro, int position) FindBestMacroToCall(ITextReader txtReader, State state, List<(int start, int end)> disabledRegions)
        {
            var bestMacros = _macros
                .Where(m => m.Pattern != null)
                .Select(macro => (macro, CreateRegexForMacro(macro).Match(txtReader.Current.Remainder)))
                .Where(macroWithMatch => macroWithMatch.Item2.Success)
                .Select(m => (m.macro, m.Item2.Groups[1].Index))
                .ToList();

            foreach (IDynamicMacro dynamicMacro in _macros.OfType<IDynamicMacro>())
            {
                if (dynamicMacro.CanBeInvoked(txtReader, state, out var index))
                {
                    bestMacros.Add((dynamicMacro, index));
                }
            }

            var bestMacro = bestMacros
                                .Where(b => !disabledRegions.Any(region => (region.start  <= b.Index + txtReader.Current.ColumnNumber) && (region.end >= b.Index + txtReader.Current.ColumnNumber)))
                                .OrderBy(m => m.macro.Priority)
                                .ThenBy(m => m.Index).FirstOrDefault();

            if (bestMacro == default)
            {
                return default;
            }
            else
            {
                return (bestMacro.macro, bestMacro.Index);
            }
        }

        private static string CreatePatternForMacro(IMacro macro)
        {
            if (macro.AreArgumentsRequired)
            {
                return @"(?<=\b|\W|^)" + "(" + macro.Pattern + @"\(.*" + ")";
            }
            return @"(?<=\b|\W|^)" + "(" + macro.Pattern + ")";
        }

        private Regex CreateRegexForMacro(IMacro macro)
        {
            if (!_cache.ContainsKey(macro))
            {
                _cache[macro] = new Regex(CreatePatternForMacro(macro));
            }

            return _cache[macro];
        }
    }
}

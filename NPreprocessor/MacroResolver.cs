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

            while (txtReader.Current?.Remainder != null)
            {
                var txt = txtReader.Current?.Remainder;
                var disabledRegions = _stringPattern.Matches(txt).Select(s => (s.Index, s.Index + s.Length - 1)).ToList();

                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state, disabledRegions);

                if (macroToCall != default)
                {
                    string skipped = macroToCall.position > 0 ? txt.Substring(0, macroToCall.position) : "";
                    result.Add(new TextBlock(skipped) { Column = txtReader.Current.ColumnNumber, Line = txtReader.Current.LineNumber });

                    txtReader.Current.Advance(macroToCall.position);

                    var macroResult = await macroToCall.macro.Invoke(txtReader, state);

                    if (macroResult.Any())
                    {
                        var toProcessMore = string.Join(string.Empty, macroResult.Where(m => !m.Finished).Select(m => m.Value));
                        var processed = macroResult.Where(m => m.Finished);
                        result.AddRange(processed);

                        var cascadeTextReader = new TextReader(toProcessMore, state.NewLineEnding, macroResult.Min(m => m.Line));

                        result.AddRange(Resolve(cascadeTextReader, new State
                        {
                            DefinitionPrefix = state.DefinitionPrefix,
                            Definitions = state.Definitions,
                            Mappings = state.Mappings,
                            MappingsParameters = state.MappingsParameters,
                            NewLineEnding = state.NewLineEnding,
                            Regexes = state.Regexes,
                            Stack = new Stack<IMacro>(state.Stack),
                        }).Result.Blocks);
                    }
                }
                else
                {
                    result.Add(new TextBlock(txt) { Column = txtReader.Current.ColumnNumber, Line = txtReader.Current.LineNumber });
                    txtReader.Current.Finish(false);
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

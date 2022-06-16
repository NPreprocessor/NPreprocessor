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
            List<TextBlock> lastResult;
            do
            {
                if (!txtReader.MoveNext())
                {
                    return new MacroResolverResult(result, state.NewLineEnding);
                }
                
                lastResult = await ProcessCurrentLine(txtReader, state);
                if (lastResult != null)
                {
                    AddToResult(result, lastResult);
                }
            }
            while (lastResult != null);

            return new MacroResolverResult(result, state.NewLineEnding);
        }

        private async Task<List<TextBlock>> ProcessCurrentLine(ITextReader txtReader, State state)
        {
            var currentLine = txtReader.Current;
            if (currentLine == null) return null;

            var disabledRegions = _stringPattern.Matches(currentLine.Remainder).Select(s => (s.Index, s.Index + s.Length - 1)).ToArray();

            var result = new List<TextBlock>();

            while (txtReader.Current?.Remainder != null)
            {
                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state, disabledRegions);

                if (macroToCall.macro != null)
                {
                    await ProcessMacro(txtReader, state, result, macroToCall.macro, macroToCall.position);
                }
                else
                {
                    AddToResult(result,
                        new List<TextBlock>
                        {
                            new TextBlock(txtReader.Current.Remainder)
                            {
                                Column = txtReader.Current.ColumnNumber,
                                Line = txtReader.Current.LineNumber
                            }
                        });
                    txtReader.Current.Finish();
                }
            }
            return result;
        }

        private async Task ProcessMacro(ITextReader txtReader, State state, List<TextBlock> result, IMacro macroToCall, int position)
        {
            state.Stack.Push(macroToCall);
            var before = txtReader.Current.Remainder;

            var skipped = before.Substring(0, position);
            txtReader.Current.Advance(position);

            var macroResult = await macroToCall.Invoke(txtReader, state);

            var blocks = macroResult.result;

            if (blocks != null && blocks.Any())
            {
                if (skipped != string.Empty)
                {
                    await ProcessText(state, result, skipped);
                }

                if (!macroResult.finished)
                {
                    var comments = blocks.Where(b => b is CommentBlock);
                    result.AddRange(comments);

                    var cascadeText = string.Join(string.Empty, blocks.Where(b => b is not CommentBlock).Select(l => l.Value));
                    await ProcessText(state, result, cascadeText);
                }
                else
                {
                    AddToResult(result, blocks);
                }
            }
            else
            {
                if (skipped != string.Empty)
                {
                    await ProcessText(state, result, skipped);
                }
            }

            state.Stack.Pop();
        }

        private async Task ProcessText(State state, List<TextBlock> result, string txt)
        {
            var textReader = new TextReader(txt, state.NewLineEnding);
            var textResult = await Resolve(textReader, new State()
            {
                Stack = new Stack<IMacro>(state.Stack),
                Definitions = state.Definitions,
                Mappings = state.Mappings,
                MappingsParameters = state.MappingsParameters,
                DefinitionPrefix = state.DefinitionPrefix
            });
            AddToResult(result, textResult.Blocks);
        }

        private static void AddToResult(List<TextBlock> result, List<TextBlock> toAdd)
        {
            result.AddRange(toAdd);
        }

        private (IMacro macro, int position) FindBestMacroToCall(ITextReader txtReader, State state, (int start, int end)[] disabledRegions)
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
                                .Where(b => !disabledRegions.Any(region => (region.start <= b.Index) && (region.end >= b.Index)))
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

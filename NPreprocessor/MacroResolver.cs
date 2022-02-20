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
                txtReader.MoveNext();
                
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
            var result = new List<TextBlock>();

            while (txtReader.Current?.Remainder != null)
            {
                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state);

                if (macroToCall.macro != null)
                {
                    var processResult = await ProcessMacro(txtReader, state, result, macroToCall.macro, macroToCall.position);
                    if (!processResult)
                    {
                        break;
                    }
                }
                else
                {
                    AddToResult(result, 
                        new List<TextBlock>
                        {
                            new TextBlock(txtReader.Current.Remainder)
                            {
                                Column = txtReader.Current.CurrentPosition,
                                Position = txtReader.Current.CurrentAbsolutePosition,
                                Line = txtReader.LineNumber
                            }
                        });
                    txtReader.Current.Finish();
                }
            }
            return result;
        }

        private async Task<bool> ProcessMacro(ITextReader txtReader, State state, List<TextBlock> result, IMacro macroToCall, int position)
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
                    blocks.Insert(0, new TextBlock(skipped));
                }

                if (!macroResult.finished)
                {
                    var cascadeText = string.Join(string.Empty, blocks.Select(l => l.Value));
                    var cascadeTextReader = new TextReader(cascadeText, state.NewLineEnding);
                    var cascadeResult = await Resolve(cascadeTextReader, new State() 
                    { 
                        Stack = new Stack<IMacro>(state.Stack), 
                        Definitions = state.Definitions,
                        Mappings = state.Mappings,
                        MappingsParameters = state.MappingsParameters,
                        DefinitionPrefix = state.DefinitionPrefix
                    });


                    AddToResult(result, cascadeResult.Blocks);
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
                    result.Add(new TextBlock(skipped));
                }
            }

            state.Stack.Pop();
            return true;
        }

        private static void AddToResult(List<TextBlock> result, List<TextBlock> toAdd)
        {
            result.AddRange(toAdd);
        }

        private (IMacro macro, int position) FindBestMacroToCall(ITextReader txtReader, State state)
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

            var bestMacro = bestMacros.OrderBy(m => m.Index)
                .FirstOrDefault();

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

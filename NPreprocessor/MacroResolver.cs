using NPreprocessor.Macros;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor
{
    public class MacroResolver : IMacroResolver
    {
        private readonly List<IMacro> _macros = new List<IMacro>();

        public MacroResolver(List<IMacro> macros)
        {
            _macros = macros;           
        }

        public List<IMacro> Macros { get => _macros; }

        public MacroResolverResult Resolve(ITextReader txtReader, State state = null)
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
                
                lastResult = ProcessCurrentLine(txtReader, state);
                if (lastResult != null)
                {
                    AddToResult(result, lastResult);
                }
            }
            while (lastResult != null);

            return new MacroResolverResult(result, state.NewLineEnding);
        }

        private List<TextBlock> ProcessCurrentLine(ITextReader txtReader, State state)
        {
            var currentLine = txtReader.Current;
            if (currentLine == null) return null;
            var result = new List<TextBlock>();

            while (txtReader.Current?.Remainder != null)
            {
                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state);

                if (macroToCall.macro != null)
                {
                    if (!ProcessMacro(txtReader, state, result, macroToCall.macro, macroToCall.position))
                    {
                        break;
                    }
                }
                else
                {
                    AddToResult(result, new List<TextBlock> { txtReader.Current.Remainder });
                    txtReader.Current.Finish();
                }
            }
            return result;
        }

        private bool ProcessMacro(ITextReader txtReader, State state, List<TextBlock> result, IMacro macroToCall, int position)
        {
            state.Stack.Push(macroToCall);
            var before = txtReader.Current.Remainder;

            var skipped = before.Substring(0, position);
            txtReader.Current.Consume(position);

            var macroResult = macroToCall.Invoke(txtReader, state);

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
                    var cascadeResult = Resolve(cascadeTextReader, new State() 
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
                .Select(macro => (macro, Regex.Match(txtReader.Current.Remainder,  CreatePatternForMacro(macro))))
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
                return @"(?<=\b|\W|\s|^)" + "(" + macro.Pattern + @"\(.*" + ")";
            }
            return @"(?<=\b|\W|\s|^)" + "(" + macro.Pattern + ")";
        }
    }
}

using NPreprocessor.Macros;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor
{
    public class MacroResolver : IMacroResolver
    {
        private List<IMacro> _macros = new List<IMacro>();

        public MacroResolver()
        {
            var defineMacro = new DefineMacro();
            _macros.Add(new UndefineMacro(defineMacro));
            _macros.Add(defineMacro);
            _macros.Add(new IfDefMacro(defineMacro));
            _macros.Add(new IfNDefMacro(defineMacro));
            _macros.Add(new IncludeMacro(defineMacro));
            _macros.Add(new DnlMacro());
            _macros.Add(new IncrMacro(defineMacro));
            _macros.Add(new DecrMacro(defineMacro));
        }

        public List<IMacro> Macros { get => _macros; }

        public List<string> Do(ITextReader txtReader)
        {
            return Do(txtReader, new State() {  NewLineEnding = txtReader.NewLineEnding });
        }

        public List<string> Do(ITextReader txtReader, State state)
        {
            txtReader.MoveNext();
            var currentLine = txtReader.Current;
            if (currentLine == null) return null;
            var result = new List<string>();

            if (txtReader.Current.FullLine.StartsWith("//"))
            {
                result.Add(txtReader.Current.FullLine);
                txtReader.Current.Finish();
                return result;
            }

            while (txtReader.Current?.Remainder != null && txtReader.Current?.Remainder != String.Empty)
            {
                var macroToCall = _macros.FirstOrDefault(macro => macro.CanBeUsed(txtReader, true)) ?? _macros.FirstOrDefault(macro => macro.CanBeUsed(txtReader, false));

                if (macroToCall != null)
                {
                    if (!ProcessMacro(txtReader, state, result, macroToCall))
                    {
                        break;
                    }
                }
                else
                {
                    AddToResult(result, new List<string> { txtReader.Current.Remainder }, false);
                    txtReader.Current.Finish();
                }
            }

            if (txtReader.Current?.Remainder == String.Empty)
            {
                result.Add(string.Empty);
                txtReader.Current.Finish();
            }

            return result;
        }

        public List<string> DoAll(ITextReader txtReader)
        {
            return DoAll(txtReader, new State());
        }

        public List<string> DoAll(ITextReader txtReader, State state)
        {
            var result = new List<string>();
            List<string> lastResult = null;
            do
            {
                lastResult = Do(txtReader, state);
                if (lastResult != null)
                {
                    AddToResult(result, lastResult, state.MergePoints == 0);

                    if (state.MergePoints > 0)
                    {
                        state.MergePoints -= 1;
                    }
                }
            }
            while (lastResult != null);

            return result;
        }

        private bool ProcessMacro(ITextReader txtReader, State state, List<string> result, IMacro macroToCall)
        {
            state.Stack.Push(macroToCall);
            var before = txtReader.Current.Remainder;

            var macroResult = macroToCall.Invoke(txtReader, state);

            if (!macroResult.invoked)
            {
                AddToResult(result, new List<string> { before }, false);
                state.Stack.Pop();
                return false;
            }

            var lines = macroResult.result;

            if (lines != null && lines.Count() > 0)
            {
                if (lines.Count == 1 && lines[0] == String.Empty)
                {
                    if (state.CreateNewLine || !result.Any())
                    {
                        result.Add(string.Empty);
                    }
                    state.Stack.Pop();
                    return true;
                }

                var cascadeTextReader = new TextReader(string.Join(state.NewLineEnding, lines), state.NewLineEnding);
                var cascadeResult = DoAll(cascadeTextReader, new State() { Stack = new Stack<IMacro>(state.Stack), Definitions = state.Definitions });

                AddToResult(result, cascadeResult, state.CreateNewLine);
            }

            state.Stack.Pop();
            return true;
        }

        private static void AddToResult(List<string> result, List<string> toAdd, bool createNewLine)
        {
            if (!result.Any() || createNewLine)
            {
                result.AddRange(toAdd);
            }
            else
            {
                result[result.Count - 1] += toAdd.First();
                result.AddRange(toAdd.Skip(1));
            }
        }
    }
}

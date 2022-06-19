using NPreprocessor.Input;
using NPreprocessor.Output;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIfDefMacro : IMacro
    {
        private readonly bool invert;

        public ExpandedIfDefMacro(string ifPrefix, string elsePrefix, string endIfPrefix, bool invert = false)
        {
            Pattern = ifPrefix;
            ElsePrefix = elsePrefix;
            EndIfPrefix = endIfPrefix;
            this.invert = invert;
        }

        public string Pattern { get; set; }

        public string ElsePrefix { get; }

        public string EndIfPrefix { get; }

        public bool AreArgumentsRequired => false;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            var currentLine = reader.Current.Remainder;
            int columnNumber = reader.Current.ColumnNumber;
            int lineNumber = reader.Current.LineNumber;
            

            reader.Current.Finish(keepNewLine: false);

            var prefixLength = Pattern.Length;
            var name = currentLine.Substring(prefixLength).Trim();

            var @trueLines = new List<string>();
            var @falseLines = new List<string>();

            int mode = 1;
            int count = 1;
            int elseStart = 0;
            int thenStart = lineNumber + 1;

            while (count != 0)
            {
                if (!reader.MoveNext())
                {
                    break;
                }


                string line = GetLine(reader);
                string lineTrimmed = line.TrimStart();

                if (lineTrimmed.StartsWith(Pattern))
                {
                    count++;
                }

                if (mode == 1 && count == 1 && lineTrimmed.StartsWith(ElsePrefix))
                {
                    if (elseStart == 0)
                    {
                        elseStart = reader.Current.LineNumber + 1;
                    }
                    mode = 2;
                    continue;
                }

                if (lineTrimmed.StartsWith(EndIfPrefix))
                {
                    count--;
                    if (count == 0)
                    {
                        reader.Current.Advance(reader.Current.Remainder.Length);
                        continue;
                    }
                }

                if (mode == 1)
                {
                    trueLines.Add(line);
                }

                if (mode == 2)
                {
                    falseLines.Add(line);
                }
            }

            string @true = MacroString.Escape(string.Join(string.Empty, trueLines));
            string @false = MacroString.Escape(string.Join(string.Empty, falseLines));
            
            string m4Line;
            if (!invert)
            {
                m4Line = $"ifdef(`{state.DefinitionPrefix}{name}', `{@true}', `{@false}', {thenStart}, {elseStart})";
            }
            else
            {
                m4Line = $"ifndef(`{state.DefinitionPrefix}{name}', `{@true}', `{@false}', {thenStart}, {elseStart})";
            }
            return Task.FromResult(new List<TextBlock> { new TextBlock(m4Line) { Column = columnNumber, Line = lineNumber }});
        }
        private static string GetLine(ITextReader txtReader)
        {
            var remainder = txtReader.Current.Remainder;

            bool insideQuotes = false;
            var commentIndex = remainder.IndexOf("//");

            for (var i = 0; i < commentIndex; i++)
            {
                if (remainder[i] == '\"')
                {
                    insideQuotes = !insideQuotes;
                }

            }

            if (commentIndex != -1 && !insideQuotes)
            {
                remainder = remainder.Substring(0, commentIndex);
                remainder += "\r\n";
            }
            return remainder;
        }
    }
}

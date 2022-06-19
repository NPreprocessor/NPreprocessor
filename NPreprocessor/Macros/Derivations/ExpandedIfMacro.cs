using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIfMacro : IMacro
    {
        private readonly bool invert;

        public ExpandedIfMacro(string ifPrefix, string elsePrefix, string endIfPrefix, bool invert = false)
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
            var expression = currentLine.Substring(prefixLength).Trim();

            var @trueLines = new List<string>();
            var @falseLines = new List<string>();

            int mode = 1;
            int count = 1;

            while (count != 0)
            {
                reader.MoveNext();

                if (reader.Current == null)
                {
                    return Task.FromResult<List<TextBlock>>(null);
                }
                else
                {
                    string line = GetLine(reader);
                    string lineTrimmed = line.TrimStart();

                    if (mode == 1 && count == 1 && lineTrimmed.StartsWith(ElsePrefix))
                    {
                        mode = 2;
                        continue;
                    }

                    if (lineTrimmed.StartsWith(Pattern))
                    {
                        count++;
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
            } 

            string @true = MacroString.Escape(string.Join(string.Empty, trueLines));
            string @false = MacroString.Escape(string.Join(string.Empty, falseLines));
            
            string m4Line;
            if (!invert)
            {
                m4Line = $"if(`{expression}', `{@true}', `{@false}')";
            }
            else
            {
                m4Line = $"if(`{expression}', `{@false}', `{@true}')";
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
            }
            return remainder;
        }
    }
}

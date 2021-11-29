using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public (List<string> result, bool finished) Invoke(ITextReader reader, State state)
        {
            var currentLine = reader.Current.Remainder;
            reader.Current.Finish();
            var prefixLength = Pattern.Length;
            var name = currentLine.Substring(prefixLength).Trim();

            var @trueLines = new List<string>();
            var @falseLines = new List<string>();

            int mode = 1;
            int count = 1;

            while (count != 0)
            {
                reader.MoveNext();

                if (reader.Current == null)
                {
                    return (null, false);
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
                            reader.Current.Consume(reader.Current.Remainder.Length);
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

            string @true = MacroString.Escape(string.Join(Environment.NewLine, trueLines));
            string @false = MacroString.Escape(string.Join(Environment.NewLine, falseLines));
            
            string m4Line;
            if (!invert)
            {
                m4Line = $"ifdef(`{state.DefinitionPrefix}{name}', `{@true}', `{@false}')";
            }
            else
            {
                m4Line = $"ifndef(`{state.DefinitionPrefix}{name}', `{@true}', `{@false}')";
            }
            return (new List<string> { m4Line }, false);
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

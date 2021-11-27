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

        public (List<string> result, bool finished) Invoke(ITextReader txtReader, State state)
        {
            var currentLine = txtReader.Current.Remainder;
            txtReader.Current.Finish();
            var prefixLength = Pattern.Length;
            var name = currentLine.Substring(prefixLength).Trim();

            var @trueLines = new List<string>();
            var @falseLines = new List<string>();

            int mode = 1;
            int count = 1;

            while (count != 0)
            {
                txtReader.MoveNext();

                if (txtReader.Current == null)
                {
                    return (null, false);
                }
                else
                {
                    string line = txtReader.Current.Remainder.TrimStart();
                    string lineNotTrimmed = txtReader.Current.Remainder;

                    if (mode == 1 && line.StartsWith(ElsePrefix))
                    {
                        mode = 2;
                        continue;
                    }

                    if (line.StartsWith(Pattern))
                    {
                        count++;
                    }

                    if (line.StartsWith(EndIfPrefix))
                    {
                        count--;
                        if (count == 0)
                        {
                            txtReader.Current.Consume(lineNotTrimmed.Length);
                            continue;
                        }
                    }

                    if (mode == 1)
                    {
                        trueLines.Add(lineNotTrimmed);
                    }

                    if (mode == 2)
                    {
                        falseLines.Add(lineNotTrimmed);
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
    }
}

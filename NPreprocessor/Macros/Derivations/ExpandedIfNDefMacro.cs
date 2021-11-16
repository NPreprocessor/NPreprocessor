using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIfNDefMacro : IMacro
    {
        public ExpandedIfNDefMacro(string ifnPrefix, string elsePrefix, string endIfPrefix)
        {
            Prefix = ifnPrefix;
            ElsePrefix = elsePrefix;
            EndIfPrefix = endIfPrefix;
        }

        public string Prefix { get; set; }

        public string ElsePrefix { get; }

        public string EndIfPrefix { get; }

        public (List<string> result, bool invoked) Invoke(ITextReader txtReader, State state)
        {
            var currentLine = txtReader.Current.Remainder;
            txtReader.Current.Finish();
            var prefixLength = Prefix.Length;
            var name = currentLine.Substring(prefixLength).Trim().Trim('\"');

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
                    if (mode == 1 && txtReader.Current.Remainder.StartsWith(ElsePrefix))
                    {
                        mode = 2;
                        continue;
                    }

                    if (txtReader.Current.Remainder.StartsWith(Prefix))
                    {
                        count++;
                    }

                    if (txtReader.Current.Remainder.StartsWith(EndIfPrefix))
                    {
                        count--;
                        if (count == 0)
                        {
                            txtReader.Current.Consume(EndIfPrefix.Length);
                            continue;
                        }
                    }

                    if (mode == 1)
                    {
                        trueLines.Add(txtReader.Current.Remainder);
                    }

                    if (mode == 2)
                    {
                        falseLines.Add(txtReader.Current.Remainder);
                    }
                }
            }

            string m4Line = $"ifndef(`{name}', `{MacroString.Escape(string.Join(Environment.NewLine, trueLines))}', `{MacroString.Escape(string.Join(Environment.NewLine, falseLines))}')";

            return (new List<string> { m4Line }, true);
        }

        public bool CanBeUsed(ITextReader txtReader, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(txtReader.Current.Remainder, $"^{Prefix}\b");
            }

            return Regex.IsMatch(txtReader.Current.Remainder, $@"{Prefix}");
        }
    }
}

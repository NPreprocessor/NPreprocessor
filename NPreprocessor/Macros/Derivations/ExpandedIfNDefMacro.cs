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

        public (List<string> result, bool invoked) Invoke(ILineReader reader, ITextReader txtReader, State state)
        {
            var currentLine = reader.Current;
            reader.Finish();
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
                    if (mode == 1 && txtReader.Current.StartsWith(ElsePrefix))
                    {
                        mode = 2;
                        continue;
                    }

                    if (txtReader.Current.StartsWith(Prefix))
                    {
                        count++;
                    }

                    if (txtReader.Current.StartsWith(EndIfPrefix))
                    {
                        count--;
                        if (count == 0)
                        {
                            continue;
                        }
                    }

                    if (mode == 1)
                    {
                        trueLines.Add(txtReader.Current);
                    }

                    if (mode == 2)
                    {
                        falseLines.Add(txtReader.Current);
                    }
                }
            }

            string m4Line = $"ifndef(`{name}', `{MacroString.Escape(string.Join(Environment.NewLine, trueLines))}', `{MacroString.Escape(string.Join(Environment.NewLine, falseLines))}')";

            return (new List<string> { m4Line }, true);
        }

        public bool CanBeUsed(ILineReader currentLine, bool atStart)
        {
            if (atStart)
            {
                return Regex.IsMatch(currentLine.Current, $"^{Prefix}\b");
            }

            return Regex.IsMatch(currentLine.Current, $@"{Prefix}");
        }
    }
}

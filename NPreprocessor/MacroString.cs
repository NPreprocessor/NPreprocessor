using NPreprocessor.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor
{
    public static class MacroString
    {
        public static string Trim(string value)
        {
            if (value.TrimStart().StartsWith('`') && value.TrimEnd().EndsWith('\''))
            {
                value = value.TrimStart().TrimEnd();

                if (value.StartsWith('`'))
                {
                    value = value.Substring(1);
                }

                if (value.EndsWith('\''))
                {
                    value = value.Substring(0, value.Length - 1);
                }
            }


            return value;
        }
        public static string Escape(string @string)
        {
            @string = @string.Replace("\r", @"\r");
            @string = @string.Replace("\n", @"\n");
            @string = @string.Replace("'", @"\'");

            return @string;
        }

        public static List<string> GetLines(string @line, string newLineEnding)
        {
            var trimmed = MacroString.Trim(line);
            var result = trimmed;

            result = result.Replace(@"\r", "\r");
            result = result.Replace(@"\n", "\n");

            return new List<string>(result.Split(newLineEnding));
        }

        public static List<TextBlock> GetBlocks(string @line, string newLineEnding)
        {
            var lines = GetLines(line, newLineEnding);

            var linesPrim = new List<string>();
            for (var i = 0; i < lines.Count; i++)
            {
                linesPrim.Add(lines[i]);
                if (i != lines.Count - 1)
                {
                    linesPrim.Add(newLineEnding);
                }
            }

            return new List<TextBlock>(linesPrim.Select(l => new TextBlock(l)));
        }
    }
}

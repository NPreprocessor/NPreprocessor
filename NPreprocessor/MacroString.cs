using System;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class MacroString
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
            @string = @string.Replace("\r", "\\r");
            @string = @string.Replace("\n", "\\n");
            return @string;
        }

        public static List<string> GetLines(string @line)
        {
            var trimmed = MacroString.Trim(line);
            var result = trimmed;

            result = result.Replace("\\r", "\r");
            result = result.Replace("\\n", "\n");

            return new List<string>(result.Split(Environment.NewLine));
        }
    }
}

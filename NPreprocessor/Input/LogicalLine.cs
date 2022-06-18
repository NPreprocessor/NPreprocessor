using System;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor.Input
{
    public class LogicalLine
    {
        public LogicalLine(List<RealLine> lines)
        {
            Lines = lines;
        }

        public List<RealLine> Lines { get; }


        public string Ending => Lines[Lines.Count - 1].Ending;

        public string Text => string.Join(Environment.NewLine, Lines.Select(l => l.Text));
    }
}

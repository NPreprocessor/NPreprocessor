using System.Collections.Generic;

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
    }
}

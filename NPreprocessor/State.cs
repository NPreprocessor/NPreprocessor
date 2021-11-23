using NPreprocessor.Macros;
using System;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class State
    {
        public int MergePoints { get; set; } = 0;

        public bool CreateNewLine => MergePoints == 0;

        public string NewLineEnding { get; set; } = Environment.NewLine;

        public Stack<IMacro> Stack { get; set; } = new Stack<IMacro>();

        public HashSet<string> Definitions { get; set; } = new HashSet<string>();
    }
}

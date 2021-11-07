using NPreprocessor.Macros;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class State
    {
        public int MergePoints { get; set; } = 0;

        public bool CreateNewLine => MergePoints == 0;

        public Stack<IMacro> Stack { get; set; } = new Stack<IMacro>();

    }
}

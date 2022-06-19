using NPreprocessor.Macros;
using System;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class State
    {
        public State()
        {
        }
        public Dictionary<string, string> Regexes { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
        
        public Dictionary<string, bool> MappingsParameters { get; set; } = new Dictionary<string, bool>();
        
        public HashSet<string> Definitions { get; set; } = new HashSet<string>();

        public string DefinitionPrefix { get; set; } = string.Empty;

        public string NewLineEnding { get; set; } = Environment.NewLine;

        public Stack<IMacro> Stack { get; set; } = new Stack<IMacro>();
    }
}

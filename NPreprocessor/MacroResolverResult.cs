using NPreprocessor.Output;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor
{
    public class MacroResolverResult
    {
        private List<TextBlock> _blocks;
        private readonly string _newLineEnding;
        private string _fullText;
        private string[] _lines;

        public MacroResolverResult(List<TextBlock> blocks, string newLineEnding)
        {
            _blocks = blocks;
            _newLineEnding = newLineEnding;
            _fullText = string.Join(string.Empty, blocks.Select(b => b.Value));
            _lines = _fullText.Split(_newLineEnding);
        }

        public int LinesCount
        {
            get
            {
                return _lines.Length; 
            }
        }

        public string this[int i]
        {
            get
            {
                if (i >= _lines.Length || i < 0)
                {
                    throw new System.Exception("Invalid index");
                }

                return _lines[i];
            }
        }

        public List<TextBlock> Blocks
        {
            get
            {
                return _blocks;
            }
        }

        public string FullText
        {
            get
            {
                return _fullText;
            }
        }
    }
}
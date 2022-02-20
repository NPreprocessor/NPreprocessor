namespace NPreprocessor.Input
{
    public class TextLineReader : ITextLineReader
    {
        private int _index = 0;

        public TextLineReader(TextLine line, string newLineCharacters)
        {
            Line = line;
            NewLineCharacters = newLineCharacters;
        }

        public string FullLine => Line.Text;

        public int CurrentPosition => _index;

        public int CurrentAbsolutePosition => _index + Line.StartPosition;

        public int LineNumber { get; set; }

        public TextLine Line { get; }

        public string NewLineCharacters { get; }

        public bool AtStart => _index == 0;

        public string Remainder
        {
            get
            {
                if (_index == 0 && FullLine.Length == 0)
                {
                    return string.Empty;
                }

                if (_index >= FullLine.Length)
                {
                    return null;
                }

                return FullLine.Substring(_index);
            }
        }

        public void Advance(int count)
        {
            _index += count;
        }

        public void Finish(bool keapNewLine = false)
        {
            if (keapNewLine)
            {
                _index = FullLine.LastIndexOf(NewLineCharacters);
                if (_index == -1)
                {
                    _index = FullLine.Length + 1;
                }
            }
            else
            {
                _index = FullLine.Length + 1;
            }
        }
    }
}

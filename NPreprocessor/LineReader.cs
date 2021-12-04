namespace NPreprocessor
{
    public class LineReader : ILineReader
    {
        private int _index;

        public LineReader(string line, string newLineCharacters)
        {
            FullLine = line;
            NewLineCharacters = newLineCharacters;
            _index = 0;
        }

        public string FullLine { get; }

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

        public void Consume(int count)
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

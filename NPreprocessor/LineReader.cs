namespace NPreprocessor
{
    public class LineReader : ILineReader
    {
        private int _index;

        public LineReader(string line)
        {
            FullLine = line;
            _index = 0;
        }

        public string FullLine { get; }

        public bool AtStart => _index == 0;

        public string Current
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

        public void Finish()
        {
            _index = FullLine.Length + 1;
        }
    }
}

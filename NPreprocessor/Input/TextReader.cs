using System.Collections;

namespace NPreprocessor.Input
{
    public class TextReader : ITextReader
    {
        private readonly string _newLineCharacters;
        private readonly char[] _textCharacters = null;
        private int _currentIndex = 0;

        public TextReader(string text, string newLineCharacters)
        {
            _newLineCharacters = newLineCharacters;
            _textCharacters = text.ToCharArray();
        }

        public string LineContinuationCharacters { get; private set; } = @"\";

        public string NewLineEnding => _newLineCharacters;

        public int LineNumber { get; private set; }

        public string CurrentLine => Current?.Remainder;

        public ITextLineReader Current { get; set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            ReadLine();

            return Current != null;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public bool AppendNext()
        {
            var current = Current;
            ReadLine();
            Current = new TextLineReader(
                new TextLine
                {
                    Text = current.Remainder + Current.Remainder,
                    StartPosition = _currentIndex,
                    LineNumber = LineNumber
                },
                NewLineEnding);

            return true;
        }

        public void Dispose()
        {
        }

        private void ReadLine()
        {
            TextLine result = ReadSingleLine();

            while (true)
            {
                int nextCurrentIndex;
                if (result != null && result.Text.EndsWith(LineContinuationCharacters + NewLineEnding))
                {
                    TextLine nextLine = PeekNextLine(out nextCurrentIndex);
                    result.Text = result.Text.Substring(0, result.Text.Length - NewLineEnding.Length - LineContinuationCharacters.Length);
                    _currentIndex = nextCurrentIndex;
                    result.Text += nextLine.Text;
                    LineNumber++;
                }
                else
                {
                    break;
                }
            }

            Current = result != null ? new TextLineReader(result, NewLineEnding) : null;
        }

        private TextLine PeekNextLine(out int nextLineIndex)
        {
            var storedCurrentIndex = _currentIndex;
            var line = ReadSingleLine();
            nextLineIndex = _currentIndex;
            _currentIndex = storedCurrentIndex;
            return line;
        }

        private TextLine ReadSingleLine()
        {
            var start = _currentIndex;

            if (_currentIndex == _textCharacters.Length)
            {
                _currentIndex++;
                return new TextLine { Text = string.Empty, LineNumber = LineNumber, StartPosition = _currentIndex };
            }

            if (_currentIndex > _textCharacters.Length)
            {
                return null;
            }

            while (_currentIndex < _textCharacters.Length)
            {
                if (StartWith(_textCharacters, _currentIndex, NewLineEnding))
                {
                    break;
                }
                _currentIndex++;
            }

            if (_currentIndex == _textCharacters.Length)
            {
                var line = new TextLine { Text = new string(_textCharacters, start, _currentIndex - start), StartPosition = _currentIndex, LineNumber = LineNumber };
                _currentIndex++;
                return line;
            }
            else
            {
                var line = new TextLine { Text = new string(_textCharacters, start, _currentIndex - start) + NewLineEnding, StartPosition = _currentIndex, LineNumber = LineNumber };
                _currentIndex += NewLineEnding.Length;

                if (_currentIndex == _textCharacters.Length)
                {
                    _currentIndex++;
                }

                LineNumber++;
                return line;
            }
        }

        private static bool StartWith(char[] textCharacters, int currentIndex, string newLineCharacters)
        {
            for (var i = 0; i < newLineCharacters.Length; i++)
            {
                if (currentIndex + i == textCharacters.Length)
                {
                    return false;
                }
                if (textCharacters[currentIndex + i] != newLineCharacters[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

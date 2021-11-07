using System;
using System.Collections;

namespace NPreprocessor
{
    public class TextReader : ITextReader
    {
        private readonly string _text = null;
        private char[] _textCharacters = null;
        private int _currentIndex = 0;
        private int _lineNumber = 0;

        public TextReader(string text)
        {
            _text = text;
            _textCharacters = text.ToCharArray();
        }

        public int LineNumber { get => _lineNumber; set => _lineNumber = value; }

        public string Current { get; set; }

        object IEnumerator.Current => Current;

        public string LineContinuationCharacters { get; set; } = @"\";

        private void ReadLine()
        {
            string result = ReadSingleLine();

            while (true)
            {
                int nextCurrentIndex;
                if (result != null && result.EndsWith(LineContinuationCharacters))
                {
                    string nextLine = PeekNextLine(out nextCurrentIndex);
                    result = result.Substring(0, result.Length - LineContinuationCharacters.Length);
                    _currentIndex = nextCurrentIndex;
                    result += nextLine;
                    LineNumber++;
                }
                else
                {
                    break;
                }
            }

            Current = result;
        }

        private string PeekNextLine(out int nextLineIndex)
        {
            var storedCurrentIndex = _currentIndex;
            var line = ReadSingleLine();
            nextLineIndex = _currentIndex;
            _currentIndex = storedCurrentIndex;
            return line;
        }

        private string ReadSingleLine()
        {
            var start = _currentIndex;

            if (_currentIndex > _textCharacters.Length)
            {
                return null;
            }

            while (_currentIndex < _textCharacters.Length
                && _textCharacters[_currentIndex] != '\n'
                && _textCharacters[_currentIndex] != '\r')
            {
                _currentIndex++;
            }
            
            if (Environment.NewLine.Length == 2 && _currentIndex < _textCharacters.Length - 1)
            {
                var forward = _textCharacters[_currentIndex].ToString() + _textCharacters[_currentIndex + 1].ToString();

                if (forward == Environment.NewLine)
                {
                    _currentIndex += Environment.NewLine.Length;
                }
            }

            if (Environment.NewLine.Length == 1 && _currentIndex < _textCharacters.Length)
            {
                var forward = _textCharacters[_currentIndex].ToString();

                if (forward == Environment.NewLine)
                {
                    _currentIndex += Environment.NewLine.Length;
                }
            }

            var line = new string(_textCharacters, start, _currentIndex - start);
            
            if (_currentIndex == _textCharacters.Length)
            {
                _currentIndex++;
            }

            LineNumber++;

            if (line.EndsWith(Environment.NewLine))
            {
                return line.Substring(0, line.Length - Environment.NewLine.Length);
            }
            return line;
        }

        public bool MoveNext()
        {
            ReadLine();

            return Current != null;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public void Dispose()
        {
        }
    }
}

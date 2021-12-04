using System;
using System.Collections;
using System.Collections.Generic;

namespace NPreprocessor
{
    public class TextReader : ITextReader
    {
        private readonly string _newLineCharacters;
        private readonly char[] _textCharacters = null;
        private int _currentIndex = 0;
        private ILineReader _lineReader = null;

        public TextReader(string text, string newLineCharacters)
        {
            _newLineCharacters = newLineCharacters;
            _textCharacters = text.ToCharArray();
        }

        public int LineNumber { get; set; }

        public string CurrentLine { get; set; }

        public string LineContinuationCharacters { get; set; } = @"\";

        private ILineReader LineReader
        {
            get
            {
                if (_lineReader == null && CurrentLine != null)
                {
                    _lineReader = new LineReader(CurrentLine, _newLineCharacters);
                }
                return _lineReader;
            }
        }

        ILineReader IEnumerator<ILineReader>.Current => LineReader;
       
        object IEnumerator.Current => LineReader;

        public string NewLineEnding => _newLineCharacters;

        private void ReadLine()
        {
            string result = ReadSingleLine();

            while (true)
            {
                int nextCurrentIndex;
                if (result != null && result.EndsWith(LineContinuationCharacters + NewLineEnding))
                {
                    string nextLine = PeekNextLine(out nextCurrentIndex);
                    result = result.Substring(0, result.Length - NewLineEnding.Length - LineContinuationCharacters.Length);
                    _currentIndex = nextCurrentIndex;
                    result += nextLine;
                    LineNumber++;
                }
                else
                {
                    break;
                }
            }

            CurrentLine = result;
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

            if (_currentIndex == _textCharacters.Length)
            {
                _currentIndex++;
                return String.Empty;
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
                var line = new string(_textCharacters, start, _currentIndex - start);
                _currentIndex++;
                return line;
            }
            else
            {
                var line = new string(_textCharacters, start, _currentIndex - start);
                line += NewLineEnding;

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

        public bool MoveNext()
        {
            ReadLine();

            _lineReader = null;

            return CurrentLine != null;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public bool AppendNext()
        {
            var current = LineReader.Remainder;
            ReadLine();
            if (CurrentLine != null)
            {
                _lineReader = new LineReader(current + CurrentLine, NewLineEnding);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
        }
    }
}

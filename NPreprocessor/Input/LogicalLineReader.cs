using System.Linq;

namespace NPreprocessor.Input
{
    public class LogicalLineReader : ILogicalLineReader
    {
        private int _currentLineIndex = 0;
        private int _currentColumnIndex = 0;

        public LogicalLineReader(LogicalLine line)
        {
            LogicalLine = line;
        }
        protected LogicalLine LogicalLine { get; }

        protected RealLine CurrentRealLine => Finished ? null : LogicalLine.Lines[_currentLineIndex];

        public bool Finished { get; set; }

        public bool KeepNewLine { get; private set; } = false;

        public string Remainder => Finished ? (KeepNewLine ? LogicalLine.Ending : null) : ((CurrentPhysicalLineReminder ?? "") + string.Join(string.Empty, LogicalLine.Lines.Skip(_currentLineIndex + 1).Select(l => l.Text)) + LogicalLine.Ending);

        public string RemainderWithoutNewLine => Finished ? null : ((CurrentPhysicalLineReminder ?? "") + string.Join(string.Empty, LogicalLine.Lines.Skip(_currentLineIndex + 1).Select(l => l.Text)));

        public string RemainderRaw => Finished ? (KeepNewLine ? LogicalLine.Ending : null) : ((CurrentPhysicalLineReminder ?? "") + (LogicalLine.Lines.Count > 1 ? ("\\" + LogicalLine.Ending) : "") +  string.Join("\\" + LogicalLine.Ending, LogicalLine.Lines.Skip(_currentLineIndex + 1).Select(l => l.Text)) + LogicalLine.Ending);

        public string CurrentPhysicalLineReminder => Finished ? null : CurrentRealLine.Text.Substring(_currentColumnIndex);

        public bool AtStart => _currentLineIndex == 0 && _currentColumnIndex == 0;

        public int ColumnNumber => _currentColumnIndex;

        public int LineNumber => Finished ? - 1 : CurrentRealLine.LineNumber;

        public void Advance(int count)
        {
            if (Finished)
            {
                if (KeepNewLine)
                {
                    KeepNewLine = false;
                }
                return;
            }

            int i = _currentLineIndex;
            while (i < LogicalLine.Lines.Count && LogicalLine.Lines[i].Text.Length < count + _currentColumnIndex)
            {
                count -= LogicalLine.Lines[i].Text.Length - _currentColumnIndex;
                _currentColumnIndex = 0;
                i++;
            }

            if (i < LogicalLine.Lines.Count)
            {
                _currentLineIndex = i;
                _currentColumnIndex = _currentColumnIndex + count;
            }
            else
            {
                Finished = true;
            }
        }

        public void Finish(bool keepNewLine = false)
        {
            Finished = true;
            KeepNewLine = keepNewLine;
            _currentLineIndex = LogicalLine.Lines.Count - 1;
            _currentColumnIndex = LogicalLine.Lines[_currentLineIndex].Text.Length;
        }
    }
}

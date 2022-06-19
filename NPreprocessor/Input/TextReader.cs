using System;
using System.Collections.Generic;
using System.Linq;

namespace NPreprocessor.Input
{
    public class TextReader : ITextReader
    {
        private readonly string _newLineCharacters;
        private readonly int _startLine;
        private int _logicalLineIndex = -1;

        public TextReader(string text, string newLineCharacters, int startLine = 1)
        {
            Text = text;
            _newLineCharacters = newLineCharacters;
            _startLine = startLine;
            Init();
        }


        public TextReader(string text) : this(text, Environment.NewLine)
        {
        }

        public string LineContinuationCharacters { get; private set; } = @"\";

        public string SingleLineComment { get; set; } = @"\\";


        public string NewLineEnding => _newLineCharacters;

        public string Text { get; }

        public LogicalLineReader Current
        {
            get;
            private set;
        }

        public List<LogicalLine> LogicalLines { get; private set; }

        public string CurrentLine => Current?.Remainder;

        public bool MoveNext()
        {
            _logicalLineIndex++;

            if (_logicalLineIndex < LogicalLines.Count)
            {
                Current = new LogicalLineReader(LogicalLines[_logicalLineIndex]);
                return true;
            }
            else
            {
                Current = null;
                return false;
            }
        }

        private void Init()
        {
            var physical = Text.Split(NewLineEnding);
            var logicalLines = new List<LogicalLine>();

            for (var i = 0; i < physical.Length; i++)
            {
                var realLine = new RealLine() { LineNumber = i + _startLine, Ending = i != physical.Length - 1 ? NewLineEnding : "" };
                if (physical[i].EndsWith(LineContinuationCharacters) && !physical[i].EndsWith(SingleLineComment))
                {
                    realLine.Text = physical[i].Substring(0, physical[i].Length - LineContinuationCharacters.Length);
                    realLine.WithContinuation = true;
                }
                else
                {
                    realLine.Text = physical[i];
                }
                var last = logicalLines.LastOrDefault();

                if (last != null && last.Lines.Last().WithContinuation)
                {
                    last.Lines.Add(realLine);
                }
                else
                {
                    logicalLines.Add(new LogicalLine(new List<RealLine>() { realLine }));
                }
            }

            LogicalLines = logicalLines;
        }
    }
}

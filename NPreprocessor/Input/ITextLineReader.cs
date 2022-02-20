namespace NPreprocessor.Input
{
    public interface ITextLineReader
    {
        string FullLine { get; }

        string Remainder { get; }

        bool AtStart { get; }

        int CurrentPosition { get; }

        int CurrentAbsolutePosition { get; }

        int LineNumber { get; set; }

        void Advance(int count);

        void Finish(bool keapNewLine = false);
    }
}

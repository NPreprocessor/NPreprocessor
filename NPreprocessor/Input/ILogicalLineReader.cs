namespace NPreprocessor.Input
{
    public interface ILogicalLineReader
    {
        string Remainder { get; }

        string RemainderRaw { get; }

        bool AtStart { get; }

        int ColumnNumber { get; }

        int LineNumber { get; }

        void Advance(int count);

        void Finish(bool keepNewLine = false);
    }
}

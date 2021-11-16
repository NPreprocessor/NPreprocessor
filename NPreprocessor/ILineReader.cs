namespace NPreprocessor
{
    public interface ILineReader
    {
        string FullLine { get; }

        string Remainder { get; }

        bool AtStart { get; }

        void Consume(int count);

        void Finish();
    }
}

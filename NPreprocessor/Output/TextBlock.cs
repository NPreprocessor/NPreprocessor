namespace NPreprocessor.Output
{
    public class TextBlock
    {
        public TextBlock(string value)
        {
            Value = value;
        }

        public bool Finished { get; set; }

        public string Value { get; set; }

        public int Column { get; set; }

        public int Line { get; set; }
    }
}

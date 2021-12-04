namespace NPreprocessor
{
    public class TextBlock
    {
        public TextBlock(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator TextBlock(string value) => new TextBlock(value);
    }
}

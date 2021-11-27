namespace NPreprocessor.Macros.Derivations
{
    public class ExpandedIfNDefMacro : ExpandedIfDefMacro
    {
        public ExpandedIfNDefMacro(string ifnPrefix, string elsePrefix, string endIfPrefix) : base(ifnPrefix, elsePrefix, endIfPrefix, true)
        {
        }
    }
}

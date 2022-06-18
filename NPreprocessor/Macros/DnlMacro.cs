using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class DnlMacro : IMacro
    {
        public string Pattern => "dnl";

        public bool AreArgumentsRequired => false;

        public int Priority { get; set; }

        public Task<List<TextBlock>> Invoke(ITextReader reader, State state)
        {
            reader.Current.Finish(false);
            return Task.FromResult(new List<TextBlock>());
        }
    }
}

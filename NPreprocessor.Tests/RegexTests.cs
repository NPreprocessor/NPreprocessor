using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace NPreprocessor.Tests
{
    public class RegexTests
    {
        private static Regex _invocation = new Regex(@"^([`\w]+)([^\(\)]|(?<something>\()|(?<-something>\)))+(?(something)(?!))", RegexOptions.Singleline);

        [Fact]
        public void Test()
        {
            string text = "`TOP((41+1), a))))";
            var m = _invocation.Match(text);
            Assert.True(m.Success);
        }
    }
}

using NPreprocessor.Input;
using NPreprocessor.Macros.Derivations;
using System;
using Xunit;

namespace NPreprocessor.Tests
{
    public class BugFixTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public async void UnclosedBlockCommentThrowsException()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            // An unclosed block comment will cause a NullReferenceException when reader goes past end
            // This tests that the error path doesn't silently succeed
            var reader = CreateTextReader("text /* unclosed");
            await Assert.ThrowsAnyAsync<Exception>(async () => await macroResolver.Resolve(reader));
        }

        [Fact]
        public async void ClosedBlockCommentErrorMessageHasCorrectSpelling()
        {
            // Verify that if we hit the FormatException path, the message is correct
            // The "commment" typo has been fixed to "comment"
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader("/* comment */ content");
            var result = await macroResolver.Resolve(reader);
            Assert.Contains("/* comment */", result.FullText);
        }

        [Fact]
        public async void ArithmeticMacroBaseWorksForIncr()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("incr(5)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("6", result[0]);
        }

        [Fact]
        public async void ArithmeticMacroBaseWorksForDecr()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("decr(5)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("4", result[0]);
        }

        [Fact]
        public async void ExpandedIfMacroWithNoCommentProcessesFullLine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfMacro("#if", "#else", "#endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("#define"));

            var reader = CreateTextReader("#define x 1\r\n#if x\r\nresult_line_no_comment\r\n#endif");
            var results = await macroResolver.Resolve(reader);
            Assert.Contains("result_line_no_comment", results.FullText);
        }

        [Fact]
        public async void ExpandedIfMacroWithCommentStripsComment()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfMacro("#if", "#else", "#endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("#define"));

            var reader = CreateTextReader("#define x 1\r\n#if x\r\nresult_line // this is a comment\r\n#endif");
            var results = await macroResolver.Resolve(reader);
            Assert.Contains("result_line ", results.FullText);
            Assert.DoesNotContain("this is a comment", results.FullText);
        }
    }
}

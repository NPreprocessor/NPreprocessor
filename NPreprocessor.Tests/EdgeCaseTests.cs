using NPreprocessor.Input;
using NPreprocessor.Macros;
using NPreprocessor.Macros.Derivations;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace NPreprocessor.Tests
{
    public class EdgeCaseTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new Input.TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public async void IncludeWithNonExistentFileThrowsException()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("include(`nonexistent_file_12345.txt')");

            await Assert.ThrowsAsync<System.IO.FileNotFoundException>(async () => await macroResolver.Resolve(reader));
        }

        [Fact]
        public async void DeeplyNestedIfDefBlocks()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateTextReader("`define a\r\n`define b\r\n`define c\r\n`ifdef a\r\n`ifdef b\r\n`ifdef c\r\ndeep_value\r\n`endif\r\n`endif\r\n`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Contains("deep_value", results.FullText);
        }

        [Fact]
        public async void MacrosWithEmptyStringArguments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, `')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("", result[0]);
        }

        [Fact]
        public async void DefineWithBacktickQuotedMultilineValues()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(greeting, `hello\r\nworld')dnl\r\ngreeting"));
            Assert.Contains("hello", result.FullText);
            Assert.Contains("world", result.FullText);
        }

        [Fact]
        public async void MultipleDnlOnConsecutiveLines()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("dnl\r\ndnl\r\ndnl\r\nresult"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("result", result[0]);
        }

        [Fact]
        public async void RegexMacroWithCaptureGroups()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader(
                "regex(`(\\w+)=(\\w+)', `Key($1) Value($2)')dnl\r\nfoo=bar"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("Key(foo) Value(bar)", result[0]);
        }

        [Fact]
        public async void RegexActionMacroWithCallback()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);

            string captured = "";
            macroResolver.Macros.Add(new RegexActionMacro(@"MAGIC_(\w+)", (Match m) => { captured = m.Groups[1].Value; }, true));

            await macroResolver.Resolve(CreateTextReader("MAGIC_test"));
            Assert.Equal("test", captured);
        }

        [Fact]
        public async void CallParserSingleArgument()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x)dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
        }

        [Fact]
        public async void CallParserNestedParentheses()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, `a(b(c))')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("a(b(c))", result[0]);
        }

        [Fact]
        public async void CallParserEscapedQuotes()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, `test')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("test", result[0]);
        }

        [Fact]
        public async void ExpandedIfDefWithNoCommentLine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateTextReader("`define flag\r\n`ifdef flag\r\nplain line without any comment\r\n`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Contains("plain line without any comment", results.FullText);
        }

        [Fact]
        public async void ExpandedIfDefWithCommentInsideQuotes()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateTextReader("`define flag\r\n`ifdef flag\r\nx = \"// not a comment\";\r\n`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Contains("// not a comment", results.FullText);
        }

        [Fact]
        public async void DefineOverwritesPreviousDefinition()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader(
                "define(x, `first')dnl\r\ndefine(x, `second')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("second", result[0]);
        }

        [Fact]
        public async void IncrWithZero()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("incr(0)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("1", result[0]);
        }

        [Fact]
        public async void DecrWithZero()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("decr(0)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("-1", result[0]);
        }

        [Fact]
        public async void IncrWithNegativeNumber()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("incr(-5)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("-4", result[0]);
        }

        [Fact]
        public async void IfNDefWithExpandedSyntax()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfNDefMacro("`ifndef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateTextReader("`ifndef UNDEFINED_FLAG\r\nincluded_content\r\n`else\r\nexcluded_content\r\n`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Contains("included_content", results.FullText);
        }

        [Fact]
        public async void BlockCommentFollowedByContent()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("/* comment */ real_content"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal(" real_content", result[0]);
        }

        [Fact]
        public async void LineCommentRemovesRestOfLine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("content // removed"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("content ", result[0]);
        }
    }
}

using NPreprocessor.Input;
using System;
using Xunit;

namespace NPreprocessor.Tests
{
    public class PerformanceTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public async void CallParserWithLongArgumentStrings()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var longArg = new string('a', 500);
            var result = await macroResolver.Resolve(CreateTextReader($"define(x, `{longArg}')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal(longArg, result[0]);
        }

        [Fact]
        public async void DefineResolveMacroWithManyMappingsReturnsCorrectResult()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            string defines = "";
            for (int i = 0; i < 20; i++)
            {
                defines += $"define(var{i}, `val{i}')dnl\r\n";
            }
            var result = await macroResolver.Resolve(CreateTextReader(defines + "var10"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("val10", result[0]);
        }

        [Fact]
        public async void RegexResolveMacroWithManyPatternsReturnsCorrectFirstMatch()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            string regexes = "";
            for (int i = 0; i < 10; i++)
            {
                regexes += $"regex(`(word{i})', `TOKEN{i}')dnl\r\n";
            }
            var result = await macroResolver.Resolve(CreateTextReader(regexes + "word5"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("TOKEN5", result[0]);
        }

        [Fact]
        public async void MultipleDefineAndResolveInSequence()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader(
                "define(a, `1')dnl\r\ndefine(b, `2')dnl\r\ndefine(c, `3')dnl\r\na b c"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("1 2 3", result[0]);
        }
    }
}

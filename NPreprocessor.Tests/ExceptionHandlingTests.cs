using NPreprocessor.Input;
using System;
using Xunit;

namespace NPreprocessor.Tests
{
    public class ExceptionHandlingTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public void MacroResolverResultInvalidIndexThrowsArgumentOutOfRange()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("hello");
            var result = macroResolver.Resolve(reader).Result;

            Assert.Throws<ArgumentOutOfRangeException>(() => result[99]);
            Assert.Throws<ArgumentOutOfRangeException>(() => result[-1]);
        }

        [Fact]
        public async void IncrWithNonNumericInputReturnsZero()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("incr(`abc')"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("0", result[0]);
        }

        [Fact]
        public async void DecrWithNonNumericInputReturnsZero()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("decr(`abc')"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("0", result[0]);
        }

        [Fact]
        public async void UnclosedBlockCommentThrowsException()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader("text /* no end");
            await Assert.ThrowsAnyAsync<Exception>(async () => await macroResolver.Resolve(reader));
        }

        [Fact]
        public async void DefineWithValidSyntaxWorks()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, `1')dnl\r\nx"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("1", result[0]);
        }

        [Fact]
        public async void RegexWithValidSyntaxWorks()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("regex(`(hello)', `Token($1)')dnl\r\nhello"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("Token(hello)", result[0]);
        }
    }
}

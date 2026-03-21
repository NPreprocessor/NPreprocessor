using NPreprocessor.Input;
using System;
using Xunit;

namespace NPreprocessor.Tests
{
    public class NullSafetyTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public async void ResolverWithEmptyInputDoesNotThrowNullReference()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader(""));
            Assert.NotNull(result);
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("", result[0]);
        }

        [Fact]
        public async void ResolverWithWhitespaceOnlyInput()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("   "));
            Assert.NotNull(result);
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("   ", result[0]);
        }

        [Fact]
        public async void UndefineWithValidArgumentWorks()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, 1)\r\nundefine(`x')\r\nx"));
            Assert.Equal(3, result.LinesCount);
            Assert.Equal("x", result[2]);
        }

        [Fact]
        public async void IfDefWithDefinedName()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, 1)\r\nifdef(`x', `yes', `no')"));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("yes", result[1]);
        }

        [Fact]
        public async void IfDefWithUndefinedNameReturnsFalseBranch()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("ifdef(`missing', `yes', `no')"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("no", result[0]);
        }

        [Fact]
        public async void IfNDefWithUndefinedNameReturnsTrueBranch()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("ifndef(`missing', `yes', `no')"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("yes", result[0]);
        }

        [Fact]
        public async void IfNDefWithDefinedNameReturnsFalseBranch()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("define(x, 1)\r\nifndef(`x', `yes', `no')"));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("no", result[1]);
        }

        [Fact]
        public async void ResolverWithNewStateCreatesDefaultState()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = await macroResolver.Resolve(CreateTextReader("hello"), null);
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("hello", result[0]);
        }
    }
}

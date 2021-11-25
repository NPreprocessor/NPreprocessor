using System;
using System.Linq;
using Xunit;

namespace NPreprocessor.Tests
{
    public class MacroResolverTests
    {
        private ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        //[Fact]
        public void VariableScenario()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, `incr(x)')dnl
x
x"));
            Assert.Equal(2, result.Count);
            Assert.Equal("0", result[0]);
            Assert.Equal("1", result[1]);
        }

        [Fact]
        public void DecrScenario1()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 1)dnl
decr(x)"));
            Assert.Single(result);
            Assert.Equal("0", result[0]); ;
        }

        [Fact]
        public void IncrScenario0()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 0)dnl
incr(4)
incr(5)"));
            Assert.Equal(2, result.Count);
            Assert.Equal("5", result[0]);
            Assert.Equal("6", result[1]);
        }

        [Fact]
        public void IncrScenario2()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 0)dnl
incr(x)
incr(x)"));
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0]);
            Assert.Equal("1", result[1]);
        }

        [Fact]
        public void DefineDefinitonResolvesToEmpty()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader("define(name1)"));
            Assert.Single(result);
            Assert.Equal(string.Empty, result[0]); ;
        }

        [Fact]
        public void DnlRemoveNewLine()
        {
            string txt = "a dnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("a test", result[0]);
        }


        [Fact]
        public void DnlRemoveNewLine2()
        {
            string txt = "a)dnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("a)test", result[0]);
        }

        [Fact]
        public void DnlRemoveNewLine3()
        {
            string txt = "define(a,1)dnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("test", result[0]);
        }

        [Fact]
        public void DnlRemoveRestOfLine()
        {
            string txt = "a dnl abc\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("a test", result[0]);
        }

        [Fact]
        public void DnlIsNotExpanded()
        {
            string txt = "adnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(2, result.Count);
            Assert.Equal("adnl", result[0]);
            Assert.Equal("test", result[1]);
        }

        [Fact]
        public void DefineResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname1");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public void DefineWithControlCharactersResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader(@"define(\Eame1, `Hello.')
\Eame1");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public void DefineCascade()
        {
            var macroResolver = new MacroResolver();
            string txt = @"define(Plus, +)dnl
define(Operator, `Plus')dnl
Operator";

            var reader = CreateTextReader(txt);
            var results = macroResolver.Resolve(reader);

            Assert.Single(results);
            Assert.Equal(" +", results[0]);
        }

        [Fact]
        public void DefineResolvesAndContinue()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')a\r\nname1");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal("a", results[0]);
            Assert.Equal("Hello.", results[1]);
        }


        [Fact]
        public void DefineDoesNotResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname12");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("name12", results[1]);
        }

        [Fact]
        public void DefineSupportsArguments()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello. $1')\r\nname1(`John') a");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello. John a", results[1]);
        }

        [Fact]
        public void UndefineWorks()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nundefine(`name1')\r\nname1");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(3, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(string.Empty, results[1]);
            Assert.Equal("name1", results[2]);
        }

        [Fact]
        public void UndefineWorksWithSpace()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\n undefine(`name1')\r\nname1");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(3, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" ", results[1]);
            Assert.Equal("name1", results[2]);
        }

        [Fact]
        public void IfDefCase1()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a, b)");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" a", results[1]);
        }


        [Fact]
        public void IfDefCase1String()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', `a', b)");
            
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("a", results[1]);
        }

        [Fact]
        public void IfDefCase2()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name2', a, b)");
            
            var results = macroResolver.Resolve(reader);
            
            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" b", results[1]);
        }

        [Fact]
        public void IfDefCase3()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a)");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(2, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" a", results[1]);
        }

        [Fact]
        public void Include()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("include(`data.txt')a");
            var result = macroResolver.Resolve(reader);
            Assert.Equal("File with `dataa", result.Single());
        }

        [Fact]
        public void DefineAndInclude()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(data, `Hello.')\r\ninclude(`data.txt')");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("File with `data", results[1]);
        }

        [Fact]
        public void DefineAndNewLines()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader(@"define(a, 1
2
3
4)dnl
`a'
a");
            var results = macroResolver.Resolve(reader);
            Assert.Equal(5, results.Count);
            Assert.Equal("`a'", results[0]);
            Assert.Equal(" 1", results[1]);
            Assert.Equal("2", results[2]);
            Assert.Equal("3", results[3]);
            Assert.Equal("4", results[4]);

        }

        [Fact]
        public void DefineAndBrackets()
        {
            var macroResolver = new MacroResolver();
     
            var reader = CreateTextReader(@"define(a, `b(()')dnl
a"); 
            var results = macroResolver.Resolve(reader);
            Assert.Single(results);
            Assert.Equal("b(()", results[0]);
        }

        [Fact]
        public void DefineAndNewLinesAndContinuations()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader(@"define(a, 1
2
3 \
4)dnl
`a'
a");
            var results = macroResolver.Resolve(reader);
            Assert.Equal(4, results.Count);
            Assert.Equal("`a'", results[0]);
            Assert.Equal(" 1", results[1]);
            Assert.Equal("2", results[2]);
            Assert.Equal("3 4", results[3]);

        }

        [Fact]
        public void LineCommentAtStart()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader(@"//include(`data.txt')");

            var results = macroResolver.Resolve(reader);

            Assert.Single(results);
            Assert.Equal("//include(`data.txt')", results[0]);
        }

        [Fact]
        public void LineCommentLaterInLine()
        {
            var macroResolver = new MacroResolver();

            var reader = CreateTextReader(@"a b c // include(`data.txt')");
            var results = macroResolver.Resolve(reader);

            Assert.Single(results);
            Assert.Equal("a b c // include(`data.txt')", results[0]);
        }
    }
}

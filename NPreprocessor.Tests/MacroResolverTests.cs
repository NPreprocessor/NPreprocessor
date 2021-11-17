using System.Linq;
using Xunit;

namespace NPreprocessor.Tests
{
    public class MacroResolverTests
    {
        private ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt);
        }

        [Fact]
        public void VariableScenario()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.DoAll(CreateTextReader(@"define(x, `incr(x)')dnl
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
            var result = macroResolver.DoAll(CreateTextReader(@"define(x, 1)dnl
decr(x)"));
            Assert.Single(result);
            Assert.Equal("0", result[0]); ;
        }

        [Fact]
        public void IncrScenario0()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.DoAll(CreateTextReader(@"define(x, 0)dnl
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
            var result = macroResolver.DoAll(CreateTextReader(@"define(x, 0)dnl
incr(x)
incr(x)"));
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0]);
            Assert.Equal("1", result[0]);
        }

        [Fact]
        public void DefineDefinitonResolvesToEmpty()
        {
            var macroResolver = new MacroResolver();
            var result = macroResolver.Do(CreateTextReader("define(name1)"));
            Assert.Single(result);
            Assert.Equal(string.Empty, result[0]); ;
        }

        [Fact]
        public void DnlRemoveNewLine()
        {
            string txt = "a dnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.DoAll(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("a test", result[0]);
        }


        [Fact]
        public void DnlRemoveRestOfLine()
        {
            string txt = "a dnl abc\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.DoAll(CreateTextReader(txt));
            Assert.Single(result);
            Assert.Equal("a test", result[0]);
        }

        [Fact]
        public void DnlIsNotExpanded()
        {
            string txt = "adnl\r\ntest";
            var macroResolver = new MacroResolver();
            var result = macroResolver.DoAll(CreateTextReader(txt));
            Assert.Equal(2, result.Count);
            Assert.Equal("adnl", result[0]);
            Assert.Equal("test", result[1]);
        }

        [Fact]
        public void DefineResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname1");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("Hello.", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void DefineWithControlCharactersResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader(@"define(\Eame1, `Hello.')
\Eame1");

            var results = macroResolver.DoAll(reader);
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

            var result = macroResolver.DoAll(reader);

            Assert.Single(result);
            Assert.Equal(" +", result[0]);
        }

        [Fact]
        public void DefineResolvesAndContinue()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')a\r\nname1");

            Assert.Equal("a", macroResolver.Do(reader)[0]);
            Assert.Equal("Hello.", macroResolver.Do(reader).Single());
        }


        [Fact]
        public void DefineDoesNotResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname12");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("name12", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void DefineSupportsArguments()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello. $1')\r\nname1(`John') a");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("Hello. John a", macroResolver.Do(reader).Single());
        }

        //[Fact]
        public void MacroCanUseZeroArgumentsResolves()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello. $0')\r\nHello. name1");

            Assert.Empty(macroResolver.Do(reader));
            Assert.Equal("Hello. name1", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void UndefineWorks()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nundefine(`name1')\r\nname1");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("name1", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void IfDefCase1()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a, b)");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal(" a", macroResolver.Do(reader).Single());
        }


        [Fact]
        public void IfDefCase1String()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', `a', b)");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("a", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void IfDefCase2()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name2', a, b)");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal(" b", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void IfDefCase3()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a)");
            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal(" a", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void Include()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("include(`data.txt')a");
            var result = macroResolver.Do(reader);
            Assert.Equal("File with dataa", result.Single());
        }

        [Fact]
        public void DefineAndInclude()
        {
            var macroResolver = new MacroResolver();
            var reader = CreateTextReader("define(data, `Hello.')\r\ninclude(`data.txt')");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            var result = macroResolver.Do(reader);
            Assert.Equal("File with Hello.", result.Single());
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
            var results = macroResolver.DoAll(reader);
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
            var results = macroResolver.DoAll(reader);
            Assert.Equal(1, results.Count);
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
            var results = macroResolver.DoAll(reader);
            Assert.Equal(4, results.Count);
            Assert.Equal("`a'", results[0]);
            Assert.Equal(" 1", results[1]);
            Assert.Equal("2", results[2]);
            Assert.Equal("3 4", results[3]);

        }
    }
}

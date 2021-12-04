using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NPreprocessor.Tests
{
    public class MacroResolverTests
    {
        private static ITextReader CreateTextReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public void RegexExample01()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(
@"regex(`if', `Token(IF)')dnl
regex(`([0-9]+)', `Token(NUMBER, $1)')dnl
if 
1234"));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("Token(IF) ", result[0]);
            Assert.Equal("Token(NUMBER, 1234)", result[1]);
        }

        [Fact]
        public void RegexExample02()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(
@"regex(`if', `Token(IF)')dnl
regex(`([0-9]+)', `Token(NUMBER, $1)')dnl
regex(`\s+', `Token(WHITESPACES)')dnl
if 1234"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("Token(IF)Token(WHITESPACES)Token(NUMBER, 1234)", result[0]);
            Assert.Equal(3, result.Blocks.Count);
            Assert.Equal("Token(IF)", result.Blocks[0].Value);
            Assert.Equal("Token(WHITESPACES)", result.Blocks[1].Value);
            Assert.Equal("Token(NUMBER, 1234)", result.Blocks[2].Value);
        }

        [Fact]
        public void DecrScenario1()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 1)dnl
decr(x)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("0", result[0]); ;
        }

        [Fact]
        public void IncrScenario0()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 0)dnl
incr(4)
incr(5)"));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("5", result[0]);
            Assert.Equal("6", result[1]);
        }

        [Fact]
        public void IncrScenario2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(@"define(x, 0)dnl
incr(x)
incr(x)"));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("1", result[0]);
            Assert.Equal("1", result[1]);
        }

        [Fact]
        public void DefineDefinitonResolvesToEmpty()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader("define(name1)"));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal(string.Empty, result[0]);
        }

        [Fact]
        public void DnlRemoveNewLine()
        {
            string txt = "a dnl\r\ntest";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("a test", result[0]);
        }


        [Fact]
        public void DnlRemoveNewLine4()
        {
            string txt = 
@"dnl
dnl
dnl
dnl
A
B";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("A", result[0]);
            Assert.Equal("B", result[1]);
        }


        [Fact]
        public void DnlRemoveNewLine2()
        {
            string txt = "a)dnl\r\ntest";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("a)test", result[0]);
        }

        [Fact]
        public void DnlRemoveNewLine3()
        {
            string txt = "define(a,1)dnl\r\ntest";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("test", result[0]);
        }

        [Fact]
        public void DnlRemoveRestOfLine()
        {
            string txt = "a dnl abc\r\ntest";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("a test", result[0]);
        }

        [Fact]
        public void DnlIsNotExpanded()
        {
            string txt = "adnl\r\ntest";
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var result = macroResolver.Resolve(CreateTextReader(txt));
            Assert.Equal(2, result.LinesCount);
            Assert.Equal("adnl", result[0]);
            Assert.Equal("test", result[1]);
        }

        [Fact]
        public void DefineResolves()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname1");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public void DefineWithControlCharactersResolves()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"define(\Eame1, `Hello.')
\Eame1");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public void DefineCascade()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            string txt = @"define(Plus, +)dnl
define(Operator, `Plus')dnl
Operator";

            var reader = CreateTextReader(txt);
            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal(" +", results[0]);
        }

        [Fact]
        public void DefineResolvesAndContinue()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')a\r\nname1");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal("a", results[0]);
            Assert.Equal("Hello.", results[1]);
        }


        [Fact]
        public void DefineDoesNotResolves()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nname12");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("name12", results[1]);
        }

        [Fact]
        public void DefineSupportsArguments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello. $1')\r\nname1(`John') a");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello. John a", results[1]);
        }

        [Fact]
        public void UndefineWorks()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nundefine(`name1')\r\nname1");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(3, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(string.Empty, results[1]);
            Assert.Equal("name1", results[2]);
        }

        [Fact]
        public void UndefineWorksWithSpace()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\n undefine(`name1')\r\nname1");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(3, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" ", results[1]);
            Assert.Equal("name1", results[2]);
        }

        [Fact]
        public void IfDefCase1()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a, b)");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" a", results[1]);
        }


        [Fact]
        public void IfDefCase1String()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', `a', b)");
            
            var results = macroResolver.Resolve(reader);

            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("a", results[1]);
        }

        [Fact]
        public void IfDefCase2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name2', a, b)");
            
            var results = macroResolver.Resolve(reader);
            
            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" b", results[1]);
        }

        [Fact]
        public void IfDefCase3()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(name1, `Hello.')\r\nifdef(`name1', a)");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" a", results[1]);
        }


        [Fact]
        public void IfDefCase4()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"define(name1,1)
ifdef(`name1', `""a\""`define y\""bc""')");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(2, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(@"""a\""`define y\""bc""", results[1]);
        }


        [Fact]
        public void Include()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("include(`data.txt')a");
            var result = macroResolver.Resolve(reader);
            
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("File with `dataa", result[0]);
        }

        [Fact]
        public void DefineAndInclude()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader("define(data, `Hello.')\r\ninclude(`data.txt')");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("File with `data", results[1]);
        }

        [Fact]
        public void DefineAndNewLines()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"define(a, 1
2
3
4)dnl
`a'
a");
            var results = macroResolver.Resolve(reader);
            Assert.Equal(5, results.LinesCount);
            Assert.Equal("`a'", results[0]);
            Assert.Equal(" 1", results[1]);
            Assert.Equal("2", results[2]);
            Assert.Equal("3", results[3]);
            Assert.Equal("4", results[4]);

        }

        [Fact]
        public void DefineAndBrackets()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
     
            var reader = CreateTextReader(@"define(a, `b(()')dnl
a"); 
            var results = macroResolver.Resolve(reader);
            Assert.Equal(1, results.LinesCount);
            Assert.Equal("b(()", results[0]);
        }

        [Fact]
        public void DefineAndNewLinesAndContinuations()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"define(a, 1
2
3 \
4)dnl
`a'
a");
            var results = macroResolver.Resolve(reader);
            Assert.Equal(4, results.LinesCount);
            Assert.Equal("`a'", results[0]);
            Assert.Equal(" 1", results[1]);
            Assert.Equal("2", results[2]);
            Assert.Equal("3 4", results[3]);

        }

        [Fact]
        public void BlockCommentAtStart()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader(@"/* include(`data.txt') */ abc");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal("/* include(`data.txt') */ abc", results[0]);
        }

        [Fact]
        public void BlockCommentLaterInLine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader(@"define(x,2)
(x - 2.0) /* include(`data.txt') */ abc");

            var results = macroResolver.Resolve(reader);

            Assert.Equal("", results[0]);
            Assert.Equal("(2 - 2.0) /* include(`data.txt') */ abc", results[1]);
        }


        [Fact]
        public void BlockCommentMulitpleLines()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader(@"dfg /* include(`data.txt')
1
2
3
*/ abc");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(@"dfg /* include(`data.txt')", results[0]);
            Assert.Equal(@"1", results[1]);
            Assert.Equal(@"2", results[2]);
            Assert.Equal(@"3", results[3]);
            Assert.Equal(@"*/ abc", results[4]);
        }

        [Fact]
        public void BlockCommentMulitpleLinesIgnored()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"dfg /* include(`data.txt')
1
2
3
*/ abc");

            var results = macroResolver.Resolve(reader);
            Assert.Equal(1, results.LinesCount);
            Assert.Equal(@"dfg  abc", results[0]);
        }

        [Fact]
        public void BlockCommentComplex()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"module mx (sin /* ... */); 
	input sin; // something
	/* ... */
endmodule");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(4, results.LinesCount);
            Assert.Equal("module mx (sin ); ", results[0]);
            Assert.Equal("	input sin; ", results[1]);
            Assert.Equal("	", results[2]);
            Assert.Equal("endmodule", results[3]);
        }

        [Fact]
        public void LineCommentAtStart()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            var reader = CreateTextReader(@"//include(`data.txt')");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal("//include(`data.txt')", results[0]);
        }

        [Fact]
        public void LineCommentIgnored()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"abc //include(`data.txt')");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal("abc ", results[0]);
        }

        [Fact]
        public void LineCommentIgnored2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"
//
//
//");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(4, results.LinesCount);
            Assert.Equal("", results[0]);
            Assert.Equal("", results[1]);
            Assert.Equal("", results[2]);
            Assert.Equal("", results[3]);
        }

        [Fact]
        public void LineCommentQuoted()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            var reader = CreateTextReader(@"define(x,1)dnl
            ""// abc"" x");

            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal(@"            ""// abc"" 1", results[0]);
        }

        [Fact]
        public void LineCommentLaterInLine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);

            var reader = CreateTextReader(@"a b c // include(`data.txt')");
            var results = macroResolver.Resolve(reader);

            Assert.Equal(1, results.LinesCount);
            Assert.Equal("a b c // include(`data.txt')", results[0]);
        }
    }
}

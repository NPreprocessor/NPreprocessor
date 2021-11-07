using NPreprocessor.Macros.Derivations;
using System.Linq;
using Xunit;

namespace NPreprocessor.Tests
{
    public class DerivationsMacroResolverTests
    {
        private ITextReader CreateLineReader(string txt)
        {
            return new TextReader(txt);
        }

        [Fact]
        public void DefineDefinitonResolvesToEmpty()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));
            var result = macroResolver.Do(CreateLineReader("`define name1"));

            Assert.Single(result);
            Assert.Equal(string.Empty, result[0]);
        }

        [Fact]
        public void DefineResolves()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader("`define name1 Hello.\r\nname1");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("Hello.", macroResolver.Do(reader).Single());
        }


        [Fact]
        public void DefineResolvesWithPrefix()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define", "`"));

            var reader = CreateLineReader("`define name1 Hello.\r\n`name1");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("Hello.", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void DefineResolvesWithContinuation()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1 Hello \
Hello.
name1");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal(@"Hello Hello.", macroResolver.Do(reader).Single());
        }


        [Fact]
        public void DefineSupportsArguments()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));
            var reader = CreateLineReader("`define name1(arg) Hello. (arg)\r\nname1(John)");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            Assert.Equal("Hello. (John)", macroResolver.Do(reader).Single());
        }

        [Fact]
        public void DefineAndInclude()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));
            macroResolver.Macros.Insert(0, new ExpandedIncludeMacro("`include"));

            var reader = CreateLineReader("`define data Hello.\r\n`include \"data.txt\"");

            Assert.Equal(string.Empty, macroResolver.Do(reader)[0]);
            var result = macroResolver.Do(reader);
            Assert.Equal("File with Hello.", result.Single());
        }


        [Fact]
        public void Include()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIncludeMacro("`include"));

            var reader = CreateLineReader("`include \"data.txt\"");
            var result = macroResolver.Do(reader);
            Assert.Equal("File with data", result.Single());
        }

        [Fact]
        public void IfDefCase1()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1
`ifdef name1
    a
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a", results[1]);
        }

        [Fact]
        public void IfNDefCase1()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfNDefMacro("`ifndef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifndef name1
    a
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a", results[1]);
        }


        [Fact]
        public void IfDefCase2()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1
`ifdef name2
    a
`else
    b
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    b", results[1]);
        }

        [Fact]
        public void IfDefCase3()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifdef name2
    a1
`ifdef name2
    a2
`endif
`else
    b
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(3, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a2", results[2]);
        }


        [Fact]
        public void IfDefCase4()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifdef name2
    a1
`ifdef name2
    a2
`ifdef name2
    a3
`endif
`endif
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(4, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a2", results[2]);
            Assert.Equal("    a3", results[3]);
        }

        [Fact]
        public void IfDefCase5()
        {
            var macroResolver = new MacroResolver();
            macroResolver.Macros.Insert(0, new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Insert(0, new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifdef name2
    a1
    a1
`ifdef name2
    a2
    a2
`ifdef name2
    a3
`endif
`endif
`endif");

            var results = macroResolver.DoAll(reader);
            Assert.Equal(6, results.Count);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a1", results[2]);
            Assert.Equal("    a2", results[3]);
            Assert.Equal("    a2", results[4]);
            Assert.Equal("    a3", results[5]);
        }
    }
}

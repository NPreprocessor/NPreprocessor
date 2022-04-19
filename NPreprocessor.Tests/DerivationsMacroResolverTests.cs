using NPreprocessor.Input;
using NPreprocessor.Macros.Derivations;
using System;
using Xunit;

namespace NPreprocessor.Tests
{
    public class DerivationsMacroResolverTests
    {
        private static ITextReader CreateLineReader(string txt)
        {
            return new TextReader(txt, Environment.NewLine);
        }

        [Fact]
        public async void DefineDefinitonResolvesToEmpty()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));
            var result = await macroResolver.Resolve(CreateLineReader("`define name1"));

            Assert.Equal(1, result.LinesCount);
            Assert.Equal(string.Empty, result[0]);
        }

        [Fact]
        public async void DefineMultilineWithArguments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define x 111.345
`define op(a,b) (a) \
* (b)
`op(`op(3,2),`x)");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`"});
            Assert.Equal(3, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(string.Empty, results[1]);
            Assert.Equal("((3) * (2)) * (111.345)", results[2]);
        }

        [Fact]
        public async void DefineResolves()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader("`define name1 Hello.\r\nname1");
            var results = await macroResolver.Resolve(reader);

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public async void DefineResolves2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(false, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader("`define NOTGIVEN 123.4\r\n val=`NOTGIVEN; // Coeff");
            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(" val=123.4; // Coeff", results[1]);
        }

        [Fact]
        public async void DefineResolvesWithComplexArguments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define Abc(nam,def,uni,lwr,upr,des) (*units=uni, type=""instance"", ask=""yes"", desc=des*) parameter real nam=def from(lwr:upr);
`Abc(pname,1,1u,0,inf,something)");
            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("(*units=1u, type=\"instance\", ask=\"yes\", desc=something*) parameter real pname=1 from(0:inf);", results[1]);
        }

        [Fact]
        public async void DefineResolvesWithComplexArguments2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define op(a, b) a + b
`op(1,4)");
            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("1 + 4", results[1]);
        }

        [Fact]
        public async void DefineResolvesWithLineComments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define abc2004 1.60217653e-19 //test
`define xyz2000 2    // test
    x     =`abc2004*`xyz2000;");
            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(string.Empty, results[1]);
            Assert.Equal("    x     =1.60217653e-19*2;", results[2]);
        }

        [Fact]
        public async void Undefine()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));
            macroResolver.Macros.Add(new ExpandedUndefineMacro("`undef"));

            var reader = CreateLineReader("`define name1 Hello.\r\n`undef name1\r\nname1");
            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(string.Empty, results[1]);
            Assert.Equal("name1", results[2]);
        }

        [Fact]
        public async void DefineResolvesWithPrefix()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader("`define name1 Hello.\r\n`name1");
            var results = await macroResolver.Resolve(reader, new State {  DefinitionPrefix = "`"});

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello.", results[1]);
        }

        [Fact]
        public async void DefineResolvesWithContinuation()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1 Hello \
Hello.
name1");
            var results = await macroResolver.Resolve(reader);

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(@"Hello Hello.", results[1]);
        }


        [Fact]
        public async void DefineSupportsArguments()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));
            var reader = CreateLineReader("`define name1(arg) Hello. (arg)\r\nname1(John)");
            var results = await macroResolver.Resolve(reader);

            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("Hello. (John)", results[1]);
        }

        [Fact]
        public async void Include()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIncludeMacro("`include"));

            var reader = CreateLineReader("`include \"data.txt\"");
            var result = await macroResolver.Resolve(reader);
            Assert.Equal(1, result.LinesCount);
            Assert.Equal("File with `data", result[0]);
        }

        [Fact]
        public async void IfDefString()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define __a__
`ifdef __a__
    ""a\""`define y\""bc""
`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal(@"    ""a\""`define y\""bc""", results[1]);
        }

        [Fact]
        public async void IfDefCase0()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define __a__
`ifdef __a__
    electrical di2, si3;
`endif");

            var results = await macroResolver.Resolve(reader, new State {  DefinitionPrefix = "`" });
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    electrical di2, si3;", results[1]);
        }

        [Fact]
        public async void IfDefCase1()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1
`ifdef name1
    a
`endif");

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a", results[1]);
        }

        [Fact]
        public async void IfNDefCase1()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfNDefMacro("`ifndef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifndef name1
    a
`endif");

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a", results[1]);
        }


        [Fact]
        public async void IfDefCase2()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name1
`ifdef name2
    a
`else
    b
`endif");

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    b", results[1]);
        }

        [Fact]
        public async void IfDefCase3()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifdef name2
    a1
`ifdef name2
    a2
`endif
`else
    b
`endif");

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(4, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a2", results[2]);
            Assert.Equal("", results[3]);
        }

        [Fact]
        public async void IfDefCase4()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

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

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(5, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a2", results[2]);
            Assert.Equal("    a3", results[3]);
            Assert.Equal("", results[4]);
        }

        [Fact]
        public async void IfDefCase5()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

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

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(7, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a1", results[2]);
            Assert.Equal("    a2", results[3]);
            Assert.Equal("    a2", results[4]);
            Assert.Equal("    a3", results[5]);
            Assert.Equal("", results[6]);
        }

        [Fact]
        public async void IfDefCase6()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define x
    `ifdef x  
    a
    `endif
end");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("        a", results[1]);
            Assert.Equal("end", results[2]);
        }


        [Fact]
        public async void IfDefCase7()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define x
    `ifdef x  
        `ifdef x
            1
        `else
            2
        `endif
    `endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("                        1", results[1]);
        }


        [Fact]
        public async void IfDefCase8()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`ifdef E1
`ifdef T1
module A(c,b,e,dt);
`else
module B(c,b,e,s,dt);
`endif
`else
`ifdef T2
module C(c,b,e);
`else
module D(c,b,e,s);
`endif
`endif");

            var results = await macroResolver.Resolve(reader, new State { DefinitionPrefix = "`" });
            Assert.Equal("module D(c,b,e,s);", results[0]);
        }

        [Fact]
        public async void IfDefCase9()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfDefMacro("`ifdef", "`else", "`endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("`define"));

            var reader = CreateLineReader(@"`define name2
`ifdef name2
    a1
    a1
`ifdef name2
    a2
    a2
`ifdef name2
    a3 dnl
`endif
`endif
`endif");

            var results = await macroResolver.Resolve(reader);
            Assert.Equal(6, results.LinesCount);
            Assert.Equal(string.Empty, results[0]);
            Assert.Equal("    a1", results[1]);
            Assert.Equal("    a1", results[2]);
            Assert.Equal("    a2", results[3]);
            Assert.Equal("    a2", results[4]);
            Assert.Equal("    a3 ", results[5]);
        }

        [Fact]
        public async void IfWorksCase01()
        {
            var macroResolver = MacroResolverFactory.CreateDefault(true, Environment.NewLine);
            macroResolver.Macros.Add(new ExpandedIfMacro("#if", "#else", "#endif"));
            macroResolver.Macros.Add(new ExpandedDefineMacro("#define"));
            var reader = CreateLineReader(@"#define x 100
#if x > 99
Console.WriteLine(""test"");
#endif");
            var results = await macroResolver.Resolve(reader);

            Assert.Equal(3, results.LinesCount);
            Assert.Equal(@"Console.WriteLine(""test"");", results[1]);
        }
    }
}

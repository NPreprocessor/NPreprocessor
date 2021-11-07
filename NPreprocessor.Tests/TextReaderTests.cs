using Xunit;

namespace NPreprocessor.Tests
{
    public class TextReaderTests
    {
        [Fact]
        public void Scenario0()
        {
            var lineReader = new TextReader("`define data Hello.\r\n`include \"data.txt\"");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("`define data Hello.", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("`include \"data.txt\"", lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }

        [Fact]
        public void Scenario1()
        {
            var lineReader = new TextReader(@"1
2
3
4");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("1", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("2", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("3", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("4", lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }

        [Fact]
        public void Scenario2()
        {
            var lineReader = new TextReader("1\\r\\n2\r\n3\\r\\n4");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("1\\r\\n2", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("3\\r\\n4", lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }

        [Fact]
        public void Scenario3()
        {
            var lineReader = new TextReader(" 1 \\r\\n 2 \r\n 3 \\r\\n 4 ");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal(" 1 \\r\\n 2 ", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal(" 3 \\r\\n 4 ", lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }

        [Fact]
        public void Scenario4()
        {
            var lineReader = new TextReader("\r\n\r\n");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("", lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal("", lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }

        [Fact]
        public void Scenario5()
        {
            var lineReader = new TextReader("");
            Assert.Null(lineReader.Current);
            lineReader.MoveNext();
            Assert.Equal(string.Empty, lineReader.Current);
            lineReader.MoveNext();
            Assert.Null(lineReader.Current);
        }
    }
}

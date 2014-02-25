
namespace PuppetLexer_UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PuppetLexer;

    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void CommonUsageTest()
        {
            var lexer = new Lexer();
            lexer.LexString("calss boo{}");
            var res = lexer.FullScan();

        }
    }
}

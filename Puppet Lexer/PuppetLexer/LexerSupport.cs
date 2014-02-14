using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetLexer
{
    public class LexerSupport
    {

    }

    public class tokenValue
    {
        public tokenValue(string[] token, int byteOffset, locator loc)
        {
                
        }
        
    }

    public class tokenQueue
    {
        public string token { get; set; }
        public tokenValue tokenValue { get; set; }
    }
    public class StringScanner
    {

        public StringScanner(string input)
        {
        }

        public int Pos { get; set; }

        public string peek(int len)
        {
            return "";
        }
        public string Scan(Regex pattern)
        {
            return "";
        }
        public string ScanUntil(Regex pattern)
        {
            return "";
        }
        public int? Skip(Regex pattern)
        {
            return null;
        }
    }
}

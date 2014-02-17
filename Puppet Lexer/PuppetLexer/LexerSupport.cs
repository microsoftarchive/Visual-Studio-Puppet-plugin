using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetLexer
{
        //# This is an integral part of the Lexer. It is broken out into a separate module
       //# for maintainability of the code, and making the various parts of the lexer focused.
        //#
    public class LexerSupport
    {
        private locator loc;
        private StringScanner scanner;
        public LexerSupport(locator locator, StringScanner scanner)
        {
            this.loc = locator;
            this.scanner = scanner;
        }
       
        public string PositionedMessage(string msg, int? pos = null)
        {
            List<string> result = new List<string> { msg };

            string file = loc.file;
            int line = loc.lineForOffset(pos != null ? pos : scanner.Pos);
            int position = loc.posOnLine(pos != null ? pos : scanner.Pos);

            if (string.IsNullOrEmpty(file))
            {
                result.Add(string.Format("in file {0}",file));
            }
            result.Add(string.Format("at line {0}:{1}",line,pos));

            return string.Join(" ",result.ToArray());
        }

        public string FollowedBy()
        {
            StringBuilder result = new StringBuilder();
            if (scanner.eos)
            {
                result.Append("<eof>");
                return result.ToString();
            }
            result.Append(scanner.rest(0, 5)).Append("...");
            result.Replace("\t", "\\t");
            result.Replace("\n", "\\n");
            result.Replace("\r", "\\r");

            return result.ToString();
        }

        public static string FormatQuote(string q)
        {
            string value = q == "'" ? "\\'" : string.Format("'{0}'", q);
            return value;
        }

        public void LexErrorWithoutPos(string msg)
        {
            //raise Puppet::LexError.new(msg) //todo
        }

        public void LexError(string msg,int ? pos)
        {
            //raise Puppet::LexError.new(positioned_message(msg, pos)); //todo
        }

        public void AssertNumeric(string value, int length)
        {
            if ((Regex.IsMatch(value, "/^0[xX].*$/")) && !(Regex.IsMatch(value, "/^0[xX][0-9A-Fa-f]+$/")))
            {
                LexError(string.Format("Not a valid hex number {0}", value), length);
            }
            else if ((Regex.IsMatch(value, "/^0[^.].*$/")) && !(Regex.IsMatch(value, "/^0[0-7]+$/")))
            {
                LexError(string.Format("Not a valid octal number {0}", value), length);
            }
            else if (!(Regex.IsMatch(value, "/0?\\d+(?:\\.\\d+)?(?:[eE]-?\\d+)?/")))
            {
                LexError(string.Format("Not a valid decimal number {0}", value), length);
            }

        }
    }

    public class tokenValue
    {
        public string[] tokenArray { get; private set; }
        public int offset { get; private set; }
        public locator loc { get; private set; }


        public tokenValue(string[] tokenArray, int offset, locator loc)
        {
            this.tokenArray = tokenArray;
            this.offset = offset;
            this.loc = loc;
        }

        public string value
        {
            get
            {
                return tokenArray[1];
            }
        }
        public string file
        {
            get
            {
                return loc.file;
            }
        }
        public int line
        {
            get
            {
                return loc.lineForOffset(offset);
            }
        }
        public int pos
        {
            get
            {
                return loc.posOnLine(offset);
            }
        }
        public int length
        {
            get
            {
                return Convert.ToInt16(tokenArray[2]);
            }
        }
        public locator locator
        {
            get
            {
                return loc;
            }
        }
    }
   
    /* ToDO - Below declared classes are just temp placeholders */

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

        public bool eos { get; set; }

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
        public string rest(int startIndex, int length)
        {
            return "";
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetLexer
{
  
    //This module is an integral part of the Lexer.
    //It defines the string slurping behavior - finding the string and non string parts in interpolated
    //strings, translating escape sequences in strings to their single character equivalence.

    public class SlurpSupport
    {
        private readonly Regex SLURP_SQ_PATTERN  = new Regex("(?:[^\\]|^|[^\\])(?:[\\]{2})*[']");
        private readonly Regex SLURP_DQ_PATTERN  = new Regex("(?:[^\\]|^|[^\\])(?:[\\]{2})*([\"]|[$]{?)");
        private readonly Regex SLURP_UQ_PATTERN  = new Regex("(?:[^\\]|^|[^\\])(?:[\\]{2})*([$]{?|z)");
        private readonly Regex SLURP_ALL_PATTERN = new Regex(".*(z)");
        private readonly string [] SQ_ESCAPES = {"'"};
        private readonly string [] DQ_ESCAPES = {"\\", "$", "'", "\"", "r", "n", "t", "s", "u", "\r\n", "\n"};
        private readonly string [] UQ_ESCAPES = {"\\", "$", "r", "n", "t", "s", "u", "\r\n", "\n"};
        
        public SlurpSupport(StringScanner scanner, Hashtable lexingContext)
        {
            this.scanner = scanner;
            this.lexingContext = lexingContext;
        }

        StringScanner scanner;
        Hashtable lexingContext;

        public string SlurpSQString() {
            scanner.Pos++;
            string str = "test"; //TODO: slurp(scanner, SLURP_SQ_PATTERN, SQ_ESCAPES, "ignore_invalid_escapes");
            if (String.IsNullOrEmpty(str)) {
            //lex_error("Unclosed quote after \"'\" followed by '#{followed_by}'") //TODO
            }
            return str.Substring(0, str.Length -2);
        }

        public string[] slurpUQString() {
            string last  = scanner.matched();
            string str = "test"; //TODO: slurp(scanner, SLURP_SQ_PATTERN, SQ_ESCAPES, "ignore_invalid_escapes");
            
            // Terminator may be a single char '$', two characters '${', or empty string '' at the end of intput.
            // If there is a terminating character is must be stripped and returned separately.
            
            string terminator = str.ElementAt(1).ToString();
            return new string [] { str.Replace(terminator, ""), terminator };
        }

        // Slurps a string from the given scanner until the given pattern and then replaces any escaped
        // characters given by escapes into their control-character equivalent or in case of line breaks, replaces the
        // pattern \r?\n with an empty string.
        // The returned string contains the terminating character. Returns nil if the scanner can not scan until the given
        // pattern.

        public string Slurp(StringScanner scanner,Regex pattern,string [] escapes, bool ignoreInvalidEscapes) {
            string str = scanner.ScanUntil(pattern);
            Regex utf8Regex = new Regex("\\u([0-9a-fAF]{4})");
            Regex escapeStringRegex = new Regex("\\([^\r\n]|(?:\r?\n))");

            if (String.IsNullOrEmpty(str)) {
                return null;
            }
            if (escapes.Contains("u"))
            {
                str = utf8Regex.Replace(str, Utf8Matcher);
            }

            str = escapeStringRegex.Replace(str, new MatchEvaluator(
                (Match m) => {
                    string escapeChar = m.Groups[0].Value;
                    string replaceChar = null;
                    if (escapes.Contains(escapeChar))
                    {
                        switch (m.Groups[0].Value)
                        {
                            case "r":
                                replaceChar = "\r";
                                break;
                            case "n":
                                replaceChar = "\n";
                                break;
                            case "t":
                                replaceChar = "\t";
                                break;
                            case "s":
                                replaceChar = " ";
                                break;
                            case "u":
                                //Puppet.warning(positioned_message("Unicode escape '\\u' was not followed by 4 hex digits")) //TODO
                                replaceChar = "\\u";
                                break;
                            case "\n":
                                replaceChar = "";
                                break;
                            case "\r\n":
                                replaceChar = "";
                                break;
                            default:
                                replaceChar = escapeChar;
                                break;
                        }
                    }
                    else if(!ignoreInvalidEscapes)
                    {
                        //Puppet.warning(positioned_message("Unrecognized escape sequence '\\#{ch}'")); //TODO
                        replaceChar = string.Format("\\{0}",escapeChar);
                    }
                    return replaceChar;
                }
            ));

            return str;
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new System.IO.StringReader(hex))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }

        private static string Utf8Matcher(Match match)
        {
           return System.Text.Encoding.UTF8.GetString(StringToByteArray(match.Groups[1].Value));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetLexer
{
    //# The Lexer is responsbile for turning source text into tokens.
    public class Lexer
    {
        // ALl tokens have three slots, the token name (a Symbol), the token text (String), and a token text length.

        private readonly string[] TOKEN_LBRACK = { "LBRACK", "[", "1" };
        private readonly string[] TOKEN_LISTSTART = { "LISTSTART", "[", "1" };
        private readonly string[] TOKEN_RBRACK = { "RBRACK", "]", "1" };
        private readonly string[] TOKEN_LBRACE = { "LBRACE", "{", "1" };
        private readonly string[] TOKEN_RBRACE = { "RBRACE", "}", "1" };
        private readonly string[] TOKEN_SELBRACE = { "SELBRACE", "{", "1" };
        private readonly string[] TOKEN_LPAREN = { "LPAREN", "(", "1" };
        private readonly string[] TOKEN_RPAREN = { "RPAREN", ")", "1" };

        private readonly string[] TOKEN_EQUALS = { "EQUALS", "=", "1" };
        private readonly string[] TOKEN_APPENDS = { "APPENDS", "+=", "2" };
        private readonly string[] TOKEN_DELETES = { "DELETES", "-=", "2" };

        private readonly string[] TOKEN_ISEQUAL = { "ISEQUAL", "==", "2" };
        private readonly string[] TOKEN_NOTEQUAL = { "NOTEQUAL", "!=", "2" };
        private readonly string[] TOKEN_MATCH = { "MATCH", "=~", "2" };
        private readonly string[] TOKEN_NOMATCH = { "NOMATCH", "!~", "2" };
        private readonly string[] TOKEN_GREATEREQUAL = { "GREATEREQUAL", ">=", "2" };
        private readonly string[] TOKEN_GREATERTHAN = { "GREATERTHAN", ">", "1" };
        private readonly string[] TOKEN_LESSEQUAL = { "LESSEQUAL", "<=", "2" };
        private readonly string[] TOKEN_LESSTHAN = { "LESSTHAN", "<", "1" };

        private readonly string[] TOKEN_FARROW = { "FARROW", "=>", "2" };
        private readonly string[] TOKEN_PARROW = { "PARROW", "+>", "2" };

        private readonly string[] TOKEN_LSHIFT = { "LSHIFT", "<<", "2" };
        private readonly string[] TOKEN_LLCOLLECT = { "LLCOLLECT", "<<|", "3" };
        private readonly string[] TOKEN_LCOLLECT = { "LCOLLECT", "<|", "2" };

        private readonly string[] TOKEN_RSHIFT = { "RSHIFT", ">>", "2" };
        private readonly string[] TOKEN_RRCOLLECT = { "RRCOLLECT", "|>>", "3" };
        private readonly string[] TOKEN_RCOLLECT = { "RCOLLECT", "|>", "2" };

        private readonly string[] TOKEN_PLUS = { "PLUS", "+", "1" };
        private readonly string[] TOKEN_MINUS = { "MINUS", "-", "1" };
        private readonly string[] TOKEN_DIV = { "DIV", "/", "1" };
        private readonly string[] TOKEN_TIMES = { "TIMES", "*", "1" };
        private readonly string[] TOKEN_MODULO = { "MODULO", "%", "1" };

        private readonly string[] TOKEN_NOT = { "NOT", "!", "1" };
        private readonly string[] TOKEN_DOT = { "DOT", ".", "1" };
        private readonly string[] TOKEN_PIPE = { "PIPE", "|", "1" };
        private readonly string[] TOKEN_AT = { "AT ", "@", "1" };
        private readonly string[] TOKEN_ATAT = { "ATAT", "@@", "2" };
        private readonly string[] TOKEN_COLON = { "COLON", ":", "1" };
        private readonly string[] TOKEN_COMMA = { "COMMA", ",", "1" };
        private readonly string[] TOKEN_SEMIC = { "SEMIC", ";", "1" };
        private readonly string[] TOKEN_QMARK = { "QMARK", "?", "1" };
        private readonly string[] TOKEN_TILDE = { "TILDE", "~", "1" };

        private readonly string[] TOKEN_REGEXP = { "REGEXP", null, "0" };

        private readonly string[] TOKEN_IN_EDGE = { "IN_EDGE", "->", "2" };
        private readonly string[] TOKEN_IN_EDGE_SUB = { "IN_EDGE_SUB", "~>", "2" };
        private readonly string[] TOKEN_OUT_EDGE = { "OUT_EDGE", "<-", "2" };
        private readonly string[] TOKEN_OUT_EDGE_SUB = { "OUT_EDGE_SUB", "<~", "2" };

        // Tokens that are always unique to what has been lexed
        private readonly string[] TOKEN_STRING = { "STRING", null, "0" };
        private readonly string[] TOKEN_DQPRE = { "DQPRE", null, "0" };
        private readonly string[] TOKEN_DQMID = { "DQPRE", null, "0" };
        private readonly string[] TOKEN_DQPOS = { "DQPRE", null, "0" };
        private readonly string[] TOKEN_NUMBER = { "NUMBER", null, "0" };
        private readonly string[] TOKEN_VARIABLE = { "VARIABLE", null, "1" };
        private readonly string[] TOKEN_VARIABLE_EMPTY = { "VARIABLE", "", "1" };

        private readonly string[] TOKEN_HEREDOC = { "HEREDOC", null, "0" };
        private readonly string[] TOKEN_EPPSTART = { "EPPSTART", null, "0" };

        private readonly string[] TOKEN_OTHER = { "OTHER", null, "0" };


        public readonly Dictionary<string, string[]> KEYWORDS = new Dictionary<string, string[]>
        {
            {"case"     , new string[] {"CASE",    "case",     "4"}},
            { "class"    ,new string[] {"CLASS",   "class",    "5"}},
            { "default"  ,new string[] {"DEFAULT", "default",  "7"}},
            { "define"   ,new string[] {"DEFINE",  "define",   "6"}},
            { "if"       ,new string[] {"IF",      "if",       "2"}},
            { "elsif"    ,new string[] {"ELSIF",   "elsif",    "5"}},
            { "else"     ,new string[] {"ELSE",    "else",     "4"}},
            { "inherits" ,new string[] {"INHERITS","inherits", "8"}},
            { "node"     ,new string[] {"NODE",    "node",     "4"}},
            { "and"      ,new string[] {"AND",     "and",      "3"}},
            { "or"       ,new string[] {"OR",      "or",       "2"}},
            { "undef"    ,new string[] {"UNDEF",   "undef",    "5"}},
            { "false"    ,new string[] {"BOOLEAN", "false",    "5"}},
            { "true"     ,new string[] {"BOOLEAN", "true",     "4"}},
            { "in"       ,new string[] {"IN",      "in",       "2"}},
            { "unless"   ,new string[] {"UNLESS",  "unless",   "6"}}
        };

        public readonly Regex PATTERN_WS = new Regex(@"[[:blank:]\r]+");

        private readonly Regex PATTERN_COMMENT = new Regex("#.*\r?");
        private readonly Regex PATTERN_MLCOMMENT = new Regex(@"/\*(.*?)\*/");

        private readonly Regex PATTERN_REGEX = new Regex(@"/[^/\n]*/");
        private readonly Regex PATTERN_REGEX_END = new Regex(@"/");
        private readonly Regex PATTERN_REGEX_A = new Regex(@"\A/"); // for replacement to ""
        private readonly Regex PATTERN_REGEX_Z = new Regex(@"/\Z"); // for replacement to ""
        private readonly Regex PATTERN_REGEX_ESC = new Regex(@"\\/"); // for replacement to "/"

        private readonly Regex PATTERN_CLASSREF = new Regex(@"((::){0,1}[A-Z][\w]*)+");
        private readonly Regex PATTERN_NAME = new Regex(@"((::)?[a-z][\w]*)(::[a-z][\w]*)*");

        private readonly Regex PATTERN_DOLLAR_VAR = new Regex(@"\$(::)?(\w+::)*\w+");
        private readonly Regex PATTERN_NUMBER = new Regex(@"\b(?:0[xX][0-9A-Fa-f]+|0?\d+(?:\.\d+)?(?:[eE]-?\d+)?)\b");

        private readonly string STRING_BSLASH_BSLASH = "\\";

        private locator locator;
        Hashtable lexingContext = new Hashtable();
        ArrayList tokenQueue;
        StringScanner scanner;

        //Temperory declarations
        private readonly string UQ_ESCAPES = "";
        private readonly string SLURP_UQ_PATTERN = "";
        private readonly string SLURP_ALL_PATTERN = "";
        //---------------------------

        public Lexer()
        {
            throw new NotImplementedException();
        }

        private void Clear()
        {
            locator = null;
            scanner = null;
            lexingContext = null;
        }

        public void LexString(string input, string path = "")
        {
            Initvars();
            scanner = new StringScanner(input);
            locator = new locator(input, path); //todo
        }


        public void LexUnquotedString(string input, locator locatorObj, string[] escapes, bool interpolate)
        {
            Initvars();
            if (locatorObj == null)
            {
                locator = new locator(input, string.Empty);
            }
            else
            {
                locator = locatorObj;
            }
            if (escapes == null)
            {
                lexingContext["escapes"] = escapes;
            }
            else
            {
                lexingContext["escapes"] = UQ_ESCAPES;
            }
            if (interpolate || escapes.Length > 0)
            {
                lexingContext["uq_slurp_pattern"] = SLURP_UQ_PATTERN;
            }
            else
            {
                lexingContext["uq_slurp_pattern"] = SLURP_ALL_PATTERN;
            }
        }


        private void Initvars()
        {
            tokenQueue = new ArrayList();

            lexingContext.Add("brace_count", 0);
            lexingContext.Add("after", null);
        }


        public void lexFile(string filePath)
        {
            Initvars();
            string Contents = string.Empty;

            //if(PuppetFileSystem.Exists(filePath)){
            //     Contents = PuppetFileSystem.Read(filePath);    // todo
            //}
            scanner = new StringScanner(Contents);
            locator = new locator(Contents, filePath);
        }

        // Scans all of the content and returns it in an array
        // Note that the terminating [false, false] token is included in the result.
        public Dictionary<string, tokenValue> FullScan()
        {
            Dictionary<string, tokenValue> result = new Dictionary<string, tokenValue>();

            Scan((token, tokenValue) => { result.Add(token, tokenValue); });
            return result;
        }

        public void Scan(Action<string, tokenValue> expr)
        {
            //if (scanner == null)
            //{
            //    // lex_error_without_pos("Internal Error: No string or file given to lexer to process.");  //todo
            //}

            //expr("token", new tokenValue());
        }

        public ArrayList LexToken()
        {
            int before = scanner.Pos;

            string la;
            la = scanner.peek(3);

            char la0 = la[0];
            char la1 = la[1];
            char la2 = la[2];

            string[] token;
            string value;
            ArrayList lexedToken = new ArrayList();
            

            switch (la0)
            {
                case '.':
                    lexedToken = Emit(TOKEN_DOT, before);
                    break;
                case ',':
                    lexedToken = Emit(TOKEN_COMMA, before);
                    break;
                case '[':
                    if (((string)lexingContext["after"] == "NAME") && (before == 0 /* || scanner.string[before-1,1] =~ /[[:blank:]\r\n]+/)*/ )){
                        lexedToken = Emit(TOKEN_LISTSTART, before);
                    }
                    else{
                        lexedToken = Emit(TOKEN_LBRACK, before);
                    }
                    break;
                case ']':
                    lexedToken = Emit(TOKEN_RBRACK, before);
                    break;
                case '(':
                    lexedToken = Emit(TOKEN_LPAREN, before);
                    break;
                case ')':
                    lexedToken = Emit(TOKEN_RPAREN, before);
                    break;
                case ';':
                    lexedToken = Emit(TOKEN_SEMIC, before);
                    break;
                case '?':
                    lexedToken = Emit(TOKEN_QMARK, before);
                    break;
                case '*':
                    lexedToken = Emit(TOKEN_TIMES, before);
                    break;
                case '%':
                    if (la1 == '>' && (bool)lexingContext["epp_mode"]){
                        scanner.Pos += 2;
                        lexingContext["epp_mode"] = "text";
                        //interpolate_epp(); //todo
                    }
                    else{
                        lexedToken = Emit(TOKEN_MODULO, before);
                    }
                    break;
                case '{':
                    // The lexer needs to help the parser since the technology used cannot deal with
                    // lookahead of same token with different precedence. This is solved by making left brace
                    // after ? into a separate token.
                    //
                    lexingContext["brace_count"] = (int)lexingContext["brace_count"] + 1;
                    
                    if (((string)lexingContext["after"]) == "QMARK"){
                         token = TOKEN_SELBRACE;
                    }
                    else{
                        token = TOKEN_LBRACE;
                    }
                    lexedToken = Emit(token, before);
                    break;
                case '}':
                    lexingContext["brace_count"] = (int)lexingContext["brace_count"] - 1;
                    lexedToken = Emit(TOKEN_RBRACE, before);
                    break;
                    
                // TOKENS @, @@, @(
                case '@':
                    switch (la1)
                    {
                        case '@':
                            lexedToken = Emit(TOKEN_ATAT, before); 
                            break;
                        case '(':
                            //heredoc(); todo
                            break;
                        default:
                            lexedToken = Emit(TOKEN_AT, before);
                            break;
                    }
                    break;

                // TOKENS |, |>, |>>
                case '|':
                    switch (la1)
                    {
                        case '>':
                            token = la2 == '>' ? TOKEN_RRCOLLECT : TOKEN_RCOLLECT;
                            lexedToken = Emit(token, before); 
                            break;
                        default:
                            lexedToken = Emit(TOKEN_PIPE, before);
                            break;
                    }
                    break;
               // TOKENS =, =>, ==, =~
                case '=':
                    switch(la1)
                    {
                        case '=':
                            token = TOKEN_ISEQUAL;
                            break;
                        case '>':
                            token = TOKEN_FARROW;
                            break;
                        case '~':
                            token = TOKEN_MATCH;
                            break;
                        default:
                            token = TOKEN_EQUALS;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;

                // TOKENS '+', '+=', and '+>'
                case '+':
                    switch (la1)
                    {
                        case '=':
                            token = TOKEN_APPENDS;
                            break;
                        case '>':
                            token = TOKEN_PARROW;
                            break;
                        default:
                            token = TOKEN_PLUS;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;

                // # TOKENS '-', '->', and epp '-%>' (end of interpolation with trim)
                case '-':
                    if ((bool)lexingContext["epp_mode"] && la1 == '%' && la2 == '>')
                    {
                        scanner.Pos += 3;
                        //interpolate_epp("with_trim"); //todo
                    }
                    switch (la1)
                    {
                        case '>':
                            token = TOKEN_IN_EDGE;
                            break;
                        case '=':
                            token = TOKEN_DELETES;
                            break;
                        default:
                            token = TOKEN_MINUS;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;

                case '!':
                    switch (la1)
                    {
                        case '=':
                            token = TOKEN_NOTEQUAL;
                            break;
                        case '~':
                            token = TOKEN_NOMATCH;
                            break;
                        default:
                            token = TOKEN_NOT;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;

                case '~':
                    lexedToken = Emit(la1 == '>' ? TOKEN_IN_EDGE_SUB : TOKEN_TILDE, before);
                    break;
                case '#':
                    //scanner.skip(PATTERN_COMMENT); //todo
                case '/':
                    switch (la1)
                    {
                        case '*':
                            //scanner.skip(PATTERN_MLCOMMENT);
                            break;
                        default:
                            value = scanner.Scan(PATTERN_REGEX);
                            if (RegexpAcceptable() && string.IsNullOrEmpty(value))
                            {
                                while (value.Substring(value.Length - 2, 1) == STRING_BSLASH_BSLASH)
                                {
                                    value = value + scanner.ScanUntil(PATTERN_REGEX_END);
                                }
                                string regex = PATTERN_REGEX_A.Replace(value, "", 1);
                                regex = PATTERN_REGEX_Z.Replace(regex, "", 1);
                                regex = PATTERN_REGEX_ESC.Replace(regex, "/");
                                lexedToken = EmitCompleted(new string[] { "REGEX", regex, Convert.ToString(scanner.Pos - before) }, before);
                            }
                            else
                            {
                                lexedToken = Emit(TOKEN_DIV, before);
                            }
                            break;
                    }
                    break;
                    // # TOKENS <, <=, <|, <<|, <<, <-, <~
                case '<':
                    switch (la1)
                    {
                        case '<':
                            if (la2 == '|')
                                token = TOKEN_LLCOLLECT;
                            else
                                token = TOKEN_LSHIFT;
                            break;
                        case '=':
                            token = TOKEN_LESSEQUAL;
                            break;
                        case '|':
                            token = TOKEN_LCOLLECT;
                            break;
                        case '-':
                            token = TOKEN_OUT_EDGE;
                            break;
                        case '~':
                            token = TOKEN_OUT_EDGE_SUB;
                            break;
                        default:
                            token = TOKEN_LESSTHAN;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;

                  //# TOKENS >, >=, >>
                case '>':
                    switch (la1)
                    {
                        case '>':
                            token = TOKEN_RSHIFT;
                            break;
                        case '=':
                            token = TOKEN_GREATEREQUAL;
                            break;
                        default:
                            token = TOKEN_GREATERTHAN;
                            break;
                    }
                    lexedToken = Emit(token, before);
                    break;
                    
                 //# TOKENS :, ::CLASSREF, ::NAME
                case ':':
                    
                    if (la1 == ':')
                    {
                        before = scanner.Pos;

                        if (la2 >= 'A' && la2 <= 'Z')
                        {
                            value = scanner.Scan(PATTERN_CLASSREF);

                            if (!string.IsNullOrEmpty(value))
                            {
                                int after = scanner.Pos;
                                lexedToken = EmitCompleted(new string[] { "CLASSREF", value, Convert.ToString(after - before) }, before);
                            }
                            else
                            {
                                //# move to faulty position ('::<uc-letter>' was ok)
                                scanner.Pos = scanner.Pos + 3;
                                //lex_error("Illegal fully qualified class reference"); //todo
                            }
                        }
                        else
                        {
                            //# NAME or error
                            value = scanner.Scan(PATTERN_NAME);
                            if (string.IsNullOrEmpty(value))
                            {
                                lexedToken = EmitCompleted(new string[] { "NAME", value, Convert.ToString(scanner.Pos - before) }, before);
                            }
                            else
                            {
                                //# move to faulty position ('::' was ok)
                                scanner.Pos = scanner.Pos + 2;
                                //lex_error("Illegal fully qualified name"); //todo
                            }
                        }
                    }
                    else
                    {
                        lexedToken = Emit(TOKEN_COLON, before);
                    }
                    break;

                case '$':
                    value = scanner.Scan(PATTERN_DOLLAR_VAR);
                    if (string.IsNullOrEmpty(value))
                    {
                        token = new string [] { "VARIABLE", value.Substring(1, value.Length - 1), Convert.ToString(scanner.Pos - before) };
                        lexedToken = EmitCompleted(token, before);
                    }
                    else
                        //# consume the $ and let higher layer complain about the error instead of getting a syntax error
                        lexedToken = Emit(TOKEN_VARIABLE_EMPTY, before);
                    break;
                case '"':
                    //# Recursive string interpolation, 'interpolate' either returns a STRING token, or
                    //# a DQPRE with the rest of the string's tokens placed in the @token_queue
                    //interpolate_dq(); todo
                    break;
                case '\'':
                    lexedToken = EmitCompleted(new string[] { "STRING"/*,slurp_sqstring*/, Convert.ToString(before - scanner.Pos) }, before);
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    value = scanner.Scan(PATTERN_NUMBER);
                      if(string.IsNullOrEmpty(value))
                      {
                       //assert_numeric(value, length); todo
                          lexedToken = EmitCompleted(new string[] { "NUMBER", value, Convert.ToString(scanner.Pos - before) }, before);
                      }
                      else{
                       // # move to faulty position ([0-9] was ok)
                        scanner.Pos = scanner.Pos + 1;
                        //lex_error("Illegal number"); todo
                      }
                    break;
                case 'a': case 'b':case 'c': case 'd':case 'e': case 'f':case 'g': case 'h':case 'i': case 'j':case 'k': case 'l':
                case 'm': case 'n':case 'o': case 'p':case 'q': case 'r':case 's': case 't':case 'u': case 'v':case 'w': case 'x':
                case 'y': case 'z':
                    value = scanner.Scan(PATTERN_NAME);
                    if(!string.IsNullOrEmpty(value))
                    {
                        token = KEYWORDS[value] != null ? KEYWORDS[value] : new string [] { "NAME", value, Convert.ToString(scanner.Pos - before) };
                        lexedToken = EmitCompleted(token, before);
                    }
                    else
                    {
                    //# move to faulty position ([a-z] was ok)
                    scanner.Pos = scanner.Pos + 1;
                    //lex_error("Illegal name"); todo
                    }
                    break;
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                    value = scanner.Scan(PATTERN_CLASSREF);
                    if(!string.IsNullOrEmpty(value))
                    {
                        token = new string[] { "CLASSREF", value, Convert.ToString(scanner.Pos - before) };
                        lexedToken = EmitCompleted(token, before);
                    }
                    else
                    {
                    //# move to faulty position ([a-z] was ok)
                    scanner.Pos = scanner.Pos + 1;
                        //lex_error("Illegal class reference"); todo
                    }
                    break;
                case '\n':
                    if ((bool)(lexingContext["newline_jump"]))
                    {
                        scanner.Pos = (int)lexingContext["newline_jump"];
                        lexingContext["newline_jump"] = null;
                    }
                    else
                    {
                        scanner.Pos += 1;
                    }
                    break;
                case ' ':case '\t':case '\r':
                    //scanner.skip(PATTERN_WS);
                    break;

                default:
                    //# In case of unicode spaces of various kinds that are captured by a regexp, but not by the
                    //# simpler case expression above (not worth handling those special cases with better performance).
                    if(scanner.Skip(PATTERN_WS) == null)
                    {
                    //# "unrecognized char"
                        lexedToken = Emit(new string[] { "OTHER", la0.ToString(), "1" }, before);
                    }
                    break;
            }

            return lexedToken;
        }

        private ArrayList Emit(string[] token, int byteOffset)
        {
           scanner.Pos = byteOffset + Convert.ToInt16(token[2]);
           return new ArrayList { token[0], new tokenValue(token, byteOffset, locator) };
        }

        public ArrayList EmitCompleted(string[] token, int byteOffset)
        {
            return new ArrayList { token[0], new tokenValue(token, byteOffset, locator) };
        }
        public void EnqueueCompleted(string[] token, int byteOffset)
        {
            tokenQueue.Add(EmitCompleted(token,byteOffset));
        }
        public void Enqueue(ArrayList emittedToken)
        {
            tokenQueue.Add(emittedToken);
        }
        private bool RegexpAcceptable()
        {
            bool value;
            switch ((string)lexingContext["after"])
            {
                case "RPAREN":
                case "RBRACK":
                case "RRCOLLECT":
                case "RCOLLECT":
                    value = false;
                    break;
                case "RBRACE":
                    value = true;
                    break;
                case "NAME":
                case "CLASSREF":
                case "NUMBER":
                case "STRING":
                case "BOOLEAN":
                case "DQPRE":
                case "DQMID":
                case "DQPOST":
                case "HEREDOC":
                case "REGEX":
                    value = false;
                    break;
                default:
                    value = true;
                    break;
            }
            return value;
        }
    }
}

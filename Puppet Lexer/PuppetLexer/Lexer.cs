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

        private readonly string [] TOKEN_LBRACK       = {"LBRACK",       "[",   "1"};
        private readonly string [] TOKEN_LISTSTART    = {"LISTSTART",    "[",   "1"};
        private readonly string [] TOKEN_RBRACK       = {"RBRACK",       "]",   "1"};
        private readonly string [] TOKEN_LBRACE       = {"LBRACE",       "{",   "1"};
        private readonly string [] TOKEN_RBRACE       = {"RBRACE",       "}",   "1"};
        private readonly string [] TOKEN_SELBRACE     = {"SELBRACE",     "{",   "1"};
        private readonly string [] TOKEN_LPAREN       = {"LPAREN",       "(",   "1"};
        private readonly string [] TOKEN_RPAREN       = {"RPAREN",       ")",   "1"};

        private readonly string [] TOKEN_EQUALS       = {"EQUALS",       "=",   "1"};
        private readonly string [] TOKEN_APPENDS      = {"APPENDS",      "+=",  "2"};
        private readonly string [] TOKEN_DELETES      = {"DELETES",      "-=",  "2"};

        private readonly string [] TOKEN_ISEQUAL      = {"ISEQUAL",      "==",  "2"};
        private readonly string [] TOKEN_NOTEQUAL     = {"NOTEQUAL",     "!=",  "2"};
        private readonly string [] TOKEN_MATCH        = {"MATCH",        "=~",  "2"};
        private readonly string [] TOKEN_NOMATCH      = {"NOMATCH",      "!~",  "2"};
        private readonly string [] TOKEN_GREATEREQUAL = {"GREATEREQUAL", ">=",  "2"};
        private readonly string [] TOKEN_GREATERTHAN  = {"GREATERTHAN",  ">",   "1"};
        private readonly string [] TOKEN_LESSEQUAL    = {"LESSEQUAL",    "<=",  "2"};
        private readonly string [] TOKEN_LESSTHAN     = {"LESSTHAN",     "<",   "1"};

        private readonly string [] TOKEN_FARROW       = {"FARROW",       "=>",  "2"};
        private readonly string [] TOKEN_PARROW       = {"PARROW",       "+>",  "2"};

        private readonly string [] TOKEN_LSHIFT       = {"LSHIFT",       "<<",  "2"};
        private readonly string [] TOKEN_LLCOLLECT    = {"LLCOLLECT",    "<<|", "3"};
        private readonly string [] TOKEN_LCOLLECT     = {"LCOLLECT",     "<|",  "2"};

        private readonly string [] TOKEN_RSHIFT       = {"RSHIFT",       ">>",  "2"};
        private readonly string [] TOKEN_RRCOLLECT    = {"RRCOLLECT",    "|>>", "3"};
        private readonly string [] TOKEN_RCOLLECT     = {"RCOLLECT",     "|>",  "2"};

        private readonly string [] TOKEN_PLUS         = {"PLUS",         "+",   "1"};
        private readonly string [] TOKEN_MINUS        = {"MINUS",        "-",   "1"};
        private readonly string [] TOKEN_DIV          = {"DIV",          "/",   "1"};
        private readonly string [] TOKEN_TIMES        = {"TIMES",        "*",   "1"};
        private readonly string [] TOKEN_MODULO       = {"MODULO",       "%",   "1"};

        private readonly string [] TOKEN_NOT          = {"NOT",          "!",   "1"};
        private readonly string [] TOKEN_DOT          = {"DOT",          ".",   "1"};
        private readonly string [] TOKEN_PIPE         = {"PIPE",         "|",   "1"};
        private readonly string [] TOKEN_AT           = {"AT ",          "@",   "1"};
        private readonly string [] TOKEN_ATAT         = {"ATAT" ,        "@@",  "2"};
        private readonly string [] TOKEN_COLON        = {"COLON",        ":",   "1"};
        private readonly string [] TOKEN_COMMA        = {"COMMA",        ",",   "1"};
        private readonly string [] TOKEN_SEMIC        = {"SEMIC",        ";",   "1"};
        private readonly string [] TOKEN_QMARK        = {"QMARK",        "?",   "1"};
        private readonly string [] TOKEN_TILDE        = {"TILDE",        "~",   "1"};

        private readonly string [] TOKEN_REGEXP       = {"REGEXP",       null,   "0"};

        private readonly string [] TOKEN_IN_EDGE      = {"IN_EDGE",      "->",  "2"};
        private readonly string [] TOKEN_IN_EDGE_SUB  = {"IN_EDGE_SUB",  "~>",  "2"};
        private readonly string [] TOKEN_OUT_EDGE     = {"OUT_EDGE",     "<-",  "2"};
        private readonly string [] TOKEN_OUT_EDGE_SUB = {"OUT_EDGE_SUB", "<~",  "2"};

        // Tokens that are always unique to what has been lexed
        private readonly string [] TOKEN_STRING         =  {"STRING", null,          "0"};
        private readonly string [] TOKEN_DQPRE          =  {"DQPRE",  null,          "0"};
        private readonly string [] TOKEN_DQMID          =  {"DQPRE",  null,          "0"};
        private readonly string [] TOKEN_DQPOS          =  {"DQPRE",  null,          "0"};
        private readonly string [] TOKEN_NUMBER         =  {"NUMBER", null,          "0"};
        private readonly string [] TOKEN_VARIABLE       =  {"VARIABLE", null,        "1"};
        private readonly string [] TOKEN_VARIABLE_EMPTY =  {"VARIABLE", "",  "1"};
            
        private readonly string [] TOKEN_HEREDOC        =  {"HEREDOC", null, "0"};
        private readonly string [] TOKEN_EPPSTART       =  {"EPPSTART", null, "0"};

        private readonly string [] TOKEN_OTHER        = {"OTHER",  null,  "0"};


        private readonly Dictionary<string, string[]> KEYWORDS = new Dictionary<string, string[]>
        {
            {"case"     , new string[]{"CASE",    "case",     "4"}},
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

        private readonly Regex PATTERN_WS        = new Regex(@"[[:blank:]\r]+");

        private readonly Regex PATTERN_COMMENT   = new Regex("#.*\r?");
        private readonly Regex PATTERN_MLCOMMENT = new Regex(@"/\*(.*?)\*/");

        private readonly Regex PATTERN_REGEX     = new Regex(@"/[^/\n]*/");
        private readonly Regex PATTERN_REGEX_END = new Regex(@"/");
        private readonly Regex PATTERN_REGEX_A   = new Regex(@"\A/"); // for replacement to ""
        private readonly Regex PATTERN_REGEX_Z   = new Regex(@"/\Z"); // for replacement to ""
        private readonly Regex PATTERN_REGEX_ESC = new Regex(@"\\/"); // for replacement to "/"

        private readonly Regex PATTERN_CLASSREF  = new Regex(@"((::){0,1}[A-Z][\w]*)+");
        private readonly Regex PATTERN_NAME      = new Regex(@"((::)?[a-z][\w]*)(::[a-z][\w]*)*");

        private readonly Regex PATTERN_DOLLAR_VAR= new Regex(@"\$(::)?(\w+::)*\w+");
        private readonly Regex PATTERN_NUMBER    = new Regex(@"\b(?:0[xX][0-9A-Fa-f]+|0?\d+(?:\.\d+)?(?:[eE]-?\d+)?)\b");

        private readonly string STRING_BSLASH_BSLASH = "\\";

        public locator locator {get;set;}
        Hashtable lexing_context = new Hashtable();

        public Lexer()
        {
             throw new NotImplementedException();
        }

        private void Clear()
        {
            locator = null;
            //scanner = null;
            //lexing_context = null;
        }

        public void Lex_String(string input, string path = "")
        {
            Initvars();
            // scanner = new StringScanner();
            locator = new locator(input,path); //todo
        }

       
        public void lex_unquoted_string(string input, locator locatorobj, string[] escapes , bool interpolate)
        {
            Initvars();
            if (locatorobj ==null){
                locator = new locator(input,string.Empty);
            }
            else{
                locator = locatorobj;
            }
            if(escapes == null){
                 //lexing_context[:escapes] = escapes;
            }
            else{
                //lexing_context[:escapes] = UQ_ESCAPES; //todo
            }
            if(interpolate || escapes.Length > 0){
                //lexing_context[:uq_slurp_pattern] = SLURP_UQ_PATTERN;
            }
            else{
                //lexing_context[:uq_slurp_pattern] = SLURP_ALL_PATTERN;
            }
        }
        

        private void Initvars()
        {
            string[] token_queue = { };

            lexing_context.Add("brace_count", 0);
            lexing_context.Add("after", null);
        }

     
        public void lex_file(string filePath)
        {
 	        Initvars();
            string Contents = string.Empty;

            //if(PuppetFileSystem.Exists(filePath)){
            //     Contents = PuppetFileSystem.Read(filePath);    // todo
            //}
            // scanner = new StringScanner(Contents);
            locator = new locator(Contents,filePath);
        }

        // Scans all of the content and returns it in an array
        // Note that the terminating [false, false] token is included in the result.
        public Dictionary<string,string> fullscan()
        {
            Dictionary<string,string> result = new Dictionary<string,string>();
            
            scan((x, y) => { result.Add(x, y); });
            return result;
        }

        public void scan(Action<string, string> expr)
        {
            expr("token", "tokenvalue");
        }

        public void lex_token()
        {
            
        }

        private void emit()
        {
            throw new NotImplementedException();
        }
        private void emit_completed()
        {
            throw new NotImplementedException();
        }
        private void enqueue_completed()
        {
            throw new NotImplementedException();
        }
        private void enqueue()
        {
            throw new NotImplementedException();
        }
        private bool regexp_acceptable()
        {
            throw new NotImplementedException();
        }
    }
}

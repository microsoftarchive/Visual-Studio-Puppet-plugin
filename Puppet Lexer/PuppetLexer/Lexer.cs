using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetLexer
{

    public class Lexer
    {
        private const string PATTERN_WS = "%r{[[:blank:]\r]+}";
        // The single line comment includes the line ending.
        private const string PATTERN_COMMENT   = "%r{#.*\r?}";
        private const string PATTERN_MLCOMMENT = "%r{/\*(.*?)\*/}m";

        private const string PATTERN_REGEX     = "%r{/[^/\n]*/}";
        private const string PATTERN_REGEX_END = "%r{/}";
        private const string PATTERN_REGEX_A   = "%r{\A/}"; // for replacement to ""
        private const string PATTERN_REGEX_Z   = "%r{/\Z}"; // for replacement to ""
        private const string PATTERN_REGEX_ESC = "%r{\\/}"; // for replacement to "/"

        // The 3x patterns:
        // PATTERN_CLASSREF       = %r{((::){0,1}[A-Z][-\w]*)+}
        // PATTERN_NAME           = %r{((::)?[a-z0-9][-\w]*)(::[a-z0-9][-\w]*)*}

        // The NAME and CLASSREF in 4x are strict. Each segment must start with
        // a letter a-z and may not contain dashes (\w includes letters, digits and _).
        //
        private const string PATTERN_CLASSREF       = "%r{((::){0,1}[A-Z][\w]*)+}";
        private const string PATTERN_NAME           = "%r{((::)?[a-z][\w]*)(::[a-z][\w]*)*}";

        private const string PATTERN_DOLLAR_VAR     = "%r{\$(::)?(\w+::)*\w+}";
        private const string PATTERN_NUMBER         = "%r{\b(?:0[xX][0-9A-Fa-f]+|0?\d+(?:\.\d+)?(?:[eE]-?\d+)?)\b}";

        public locator locator {get;set;}

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
        public void fullscan()
        {
            throw new NotImplementedException();
        }

        public void scan()
        {
            throw new NotImplementedException();
        }

        public void lex_token()
        {
            throw new NotImplementedException();
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

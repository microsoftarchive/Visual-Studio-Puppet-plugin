// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
#define BACKUP
//
// mplex.frame
// Version 0.6.1 of 1 August 2007
// Left and Right Anchored state support.
// Start condition stack. Two generic params.
// Using fixed length context handling for right anchors
//
using System;
using System.IO;
using System.Collections.Generic;
#if !STANDALONE
using Puppet.ParserGenerator;
#endif // STANDALONE

using Puppet;
using Puppet.Parser;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Package;

namespace Puppet.Lexer
{   
    /// <summary>
    /// Summary Canonical example of MPLEX automaton
    /// </summary>
    
#if STANDALONE
    //
    // These are the dummy declarations for stand-alone MPLEX applications
    // normally these declarations would come from the parser.
    // If you declare /noparser, or %option noparser then you get this.
    //

    public enum Tokens
    { 
      EOF = 0, maxParseToken = int.MaxValue 
      // must have at least these two, values are almost arbitrary
    }

    public abstract class ScanBase
    {
        public abstract int yylex();
        protected abstract int CurrentSc { get; set; }
        //
        // Override this virtual EolState property if the scanner state is more
        // complicated then a simple copy of the current start state ordinal
        //
        public virtual int EolState { get { return CurrentSc; } set { CurrentSc = value; } }
    }
    
    public interface IColorScan
    {
        void SetSource(string source, int offset);
        int GetNext(ref int state, out int start, out int end);
    }
    
    

#endif // STANDALONE

    public abstract class ScanBuff
    {
        public const int EOF = -1;
        public abstract int Pos { get; set; }
        public abstract int Read();
        public abstract int Peek();
        public abstract int ReadPos { get; }
        public abstract string GetString(int b, int e);
    }
    
    // If the compiler can't find ScanBase maybe you need
    // to run mppg with /mplex, or run mplex with /noparser
    public sealed class Scanner : ScanBase, IColorScan
    {
   
        public ScanBuff buffer;
        private IErrorHandler handler;
        int scState;
        
        private static int GetMaxParseToken() {
            System.Reflection.FieldInfo f = typeof(Tokens).GetField("maxParseToken");
            return (f == null ? int.MaxValue : (int)f.GetValue(null));
        }
        
        static int parserMax = GetMaxParseToken();        
        
        protected override int CurrentSc 
        {
             // The current start state is a property
             // to try to avoid the user error of setting
             // scState but forgetting to update the FSA
             // start state "currentStart"
             //
             get { return scState; }
             set { scState = value; currentStart = startState[value]; }
        }
        
        enum Result {accept, noMatch, contextFound};

        const int maxAccept = 72;
        const int initial = 73;
        const int eofNum = 0;
        const int goStart = -1;
        const int INITIAL = 0;
        const int COMMENT = 1;
        const int LN_COMMENT = 2;

//form lexer.lex

        internal class TokDesc
        {
            internal TokDesc(Tokens token, TokenType tokenType, string remark)
            {
                this.Token = token;
                this.TokenType = tokenType;
                this.Remark = remark;
            }

            internal Tokens Token { get; private set; }
            internal TokenType TokenType { get; private set; }
            internal string Remark { get; set; }
        }

        private static Dictionary<string, TokDesc> tokenDescMap = new Dictionary<string, TokDesc> {
            { "case", new TokDesc( Tokens.CASE, TokenType.Keyword, string.Empty ) },
            { "class", new TokDesc( Tokens.CLASS, TokenType.Keyword, string.Empty ) },
            { "default", new TokDesc( Tokens.DEFAULT, TokenType.Keyword, string.Empty ) },
            { "define", new TokDesc( Tokens.DEFINE, TokenType.Keyword, string.Empty ) },
            { "if", new TokDesc( Tokens.IF, TokenType.Keyword, string.Empty ) },
            { "elsif", new TokDesc( Tokens.ELSIF, TokenType.Keyword, string.Empty ) },
            { "else", new TokDesc( Tokens.ELSE, TokenType.Keyword, string.Empty ) },
            { "inherits", new TokDesc( Tokens.INHERITS, TokenType.Keyword, string.Empty ) }, 
            { "node", new TokDesc( Tokens.NODE, TokenType.Keyword, string.Empty ) },
            { "and", new TokDesc( Tokens.AND, TokenType.Keyword, string.Empty ) },
            { "or", new TokDesc( Tokens.OR, TokenType.Keyword, string.Empty ) },
            { "undef", new TokDesc( Tokens.UNDEF, TokenType.Keyword, string.Empty ) },
            { "true", new TokDesc( Tokens.BOOLEAN, TokenType.Keyword, string.Empty ) },
            { "false", new TokDesc( Tokens.BOOLEAN, TokenType.Keyword, string.Empty ) },
            { "in", new TokDesc( Tokens.IN, TokenType.Keyword, string.Empty ) },
            { "unless", new TokDesc( Tokens.UNLESS, TokenType.Keyword, string.Empty ) },

            { "regex", new TokDesc( Tokens.REGEX, TokenType.Literal, "regular expression" ) },
            { "/", new TokDesc( Tokens.DIV, TokenType.Operator, "divide" ) },

            { "identifier", new TokDesc(Tokens.NAME, TokenType.Identifier, "identifier") },
            { "number", new TokDesc(Tokens.NUMBER, TokenType.Literal, "number") },
            { "string", new TokDesc(Tokens.STRING, TokenType.Literal, "string") },
            { "variable", new TokDesc(Tokens.VARIABLE, TokenType.Identifier, "variable") }, 
            { "classref", new TokDesc(Tokens.CLASSREF, TokenType.Identifier, "class reference") },
            { "[", new TokDesc(Tokens.LBRACK, TokenType.Delimiter, string.Empty) },
            { "]", new TokDesc(Tokens.RBRACK, TokenType.Delimiter, string.Empty) },
            { "{", new TokDesc(Tokens.LBRACE, TokenType.Delimiter, string.Empty) },
            { "}", new TokDesc(Tokens.RBRACE, TokenType.Delimiter, string.Empty) },
            { "(", new TokDesc(Tokens.LPAREN, TokenType.Delimiter, string.Empty) },
            { ")", new TokDesc(Tokens.RPAREN, TokenType.Delimiter, string.Empty) },
            { "=", new TokDesc(Tokens.EQUALS, TokenType.Operator, string.Empty) },
            { "+=", new TokDesc(Tokens.APPENDS, TokenType.Operator, string.Empty) },
            { "-=", new TokDesc(Tokens.DELETES, TokenType.Operator, string.Empty) },
            { "==", new TokDesc(Tokens.ISEQUAL, TokenType.Operator, string.Empty) },
            { "!=", new TokDesc(Tokens.NOTEQUAL, TokenType.Operator, string.Empty) },
            { "=~", new TokDesc(Tokens.MATCH, TokenType.Operator, string.Empty) },
            { "!~", new TokDesc(Tokens.NOMATCH, TokenType.Operator, string.Empty) },
            { ">=", new TokDesc(Tokens.GREATEREQUAL, TokenType.Operator, string.Empty) },
            { ">", new TokDesc(Tokens.GREATERTHAN, TokenType.Operator, string.Empty) },
            { "<=", new TokDesc(Tokens.LESSEQUAL, TokenType.Operator, string.Empty) },
            { "<", new TokDesc(Tokens.LESSTHAN, TokenType.Operator, string.Empty) },
            { "=>", new TokDesc(Tokens.FARROW, TokenType.Operator, string.Empty) },
            { "+>", new TokDesc(Tokens.PARROW, TokenType.Operator, string.Empty) },
            { "<<", new TokDesc(Tokens.LSHIFT, TokenType.Operator, string.Empty) },
            { "<<|", new TokDesc(Tokens.LLCOLLECT, TokenType.Operator, string.Empty) },
            { "<|", new TokDesc(Tokens.LCOLLECT, TokenType.Operator, string.Empty) },
            { ">>", new TokDesc(Tokens.RSHIFT, TokenType.Operator, string.Empty) },
            { "|>>", new TokDesc(Tokens.RRCOLLECT, TokenType.Operator, string.Empty) },
            { "|>", new TokDesc(Tokens.RCOLLECT, TokenType.Operator, string.Empty) },
            { "+", new TokDesc(Tokens.PLUS, TokenType.Operator, string.Empty) },
            { "-", new TokDesc(Tokens.MINUS, TokenType.Operator, string.Empty) },
            { "*", new TokDesc(Tokens.TIMES, TokenType.Operator, string.Empty) },
            { "%", new TokDesc(Tokens.MODULO, TokenType.Operator, string.Empty) },
            { "!", new TokDesc(Tokens.NOT, TokenType.Operator, string.Empty) },
            { ".", new TokDesc(Tokens.DOT, TokenType.Operator, string.Empty) },
            { "|", new TokDesc(Tokens.PIPE, TokenType.Operator, string.Empty) },
            { "@", new TokDesc(Tokens.AT, TokenType.Operator, string.Empty) },
            { "@@", new TokDesc(Tokens.ATAT, TokenType.Operator, string.Empty) },
            { ":", new TokDesc(Tokens.COLON, TokenType.Operator, string.Empty) },
            { ",", new TokDesc(Tokens.COMMA, TokenType.Operator, string.Empty) },
            { ";", new TokDesc(Tokens.SEMIC, TokenType.Operator, string.Empty) },
            { "?", new TokDesc(Tokens.QMARK, TokenType.Operator, string.Empty) },
            { "~", new TokDesc(Tokens.TILDE, TokenType.Operator, string.Empty) },
            { "->", new TokDesc(Tokens.IN_EDGE, TokenType.Operator, string.Empty) },
            { "~>", new TokDesc(Tokens.IN_EDGE_SUB, TokenType.Operator, string.Empty) },
            { "<-", new TokDesc(Tokens.OUT_EDGE, TokenType.Operator, string.Empty) },
            { "<~", new TokDesc(Tokens.OUT_EDGE_SUB, TokenType.Operator, string.Empty) },
            { "line.comment", new TokDesc(Tokens.LN_COMMENT, TokenType.LineComment, "line comment") },
            { "block.comment", new TokDesc(Tokens.BL_COMMENT, TokenType.Comment, "block comment") },
        };

        private int GetTokenId(string value)
        {
            TokDesc res = new TokDesc(Tokens.error, TokenType.Unknown, string.Empty);
            return TokenDescMap.TryGetValue(value, out res) ? (int)res.Token : (int)Tokens.error;
        }

        internal static Dictionary<string, TokDesc> TokenDescMap
        {
            get
            {
                return Scanner.tokenDescMap;
            }
        }

        internal static TokDesc GetTokDesc(Tokens token)
        {
            var td = Scanner.TokenDescMap.FirstOrDefault(d => d.Value.Token == token);
            
            if (td.Value != null)
            {
                if (string.IsNullOrEmpty(td.Value.Remark))
                {
                    td.Value.Remark = td.Key;
                }
            }

            return td.Value;
        }

        private static int GetKeywordOrName(string txt)
        {
            if (txt.StartsWith("::"))
            {
                return (int)Tokens.NAME;
            }

            if (TokenDescMap.ContainsKey(txt))
            {
                return (int)TokenDescMap[txt].Token;
            }

            return (int)Tokens.NAME;
        }

        private int GetDivideOrRegex(string txt)
        {
            var ctx = new Context();
            SaveStateAndPos(ctx);
            int pch = txt[0];
            int ch;
            while ((ch = this.buffer.Read()) != '\n' && ch != ScanBuff.EOF)
            {
                if ((char)ch == '/' && (char)pch != '\\')
                {
                    GetChr();
                    this.MarkEnd();
                    return (int)Tokens.REGEX;
                }

                pch = ch;
            }

            RestoreStateAndPos(ctx);
            return (int)Tokens.DIV;
        }

        internal void LoadYylval()
        {
            yylval.str = tokTxt;
            yylloc = new LexLocation(tokLin, tokCol, tokLin, tokECol);
        }
       
        public override void yyerror(string s, params object[] a)
        {
            //Debug.WriteLine(string.Format("Puppet({0}:{1})", tokLin, tokCol) + ": " + string.Format(s, a));
            if (handler != null)
            { 
                handler.AddError(s, tokLin, tokCol, tokLin, tokECol);
            }
        }
        int state;
        int currentStart = initial;
        int chr;           // last character read
        int cNum;          // ordinal number of chr
        int lNum = 0;      // current line number
        int lineStartNum;  // cNum at start of line

        //
        // The following instance variables are useful, among other
        // things, for constructing the yylloc location objects.
        //
        int tokPos;        // buffer position at start of token
        int tokNum;        // ordinal number of first character
        int tokLen;        // number of character in token
        int tokCol;        // zero-based column number at start of token
        int tokLin;        // line number at start of token
        int tokEPos;       // buffer position at end of token
        int tokECol;       // column number at end of token
        int tokELin;       // line number at end of token
        string tokTxt;     // lazily constructed text of token
#if STACK          
        private Stack<int> scStack = new Stack<int>();
#endif // STACK

#region ScannerTables
    struct Table {
        public int min; public int rng; public int dflt;
        public sbyte[] nxt;
        public Table(int m, int x, int d, sbyte[] n) {
            min = m; rng = x; dflt = d; nxt = n;
        }
    };

    static int[] startState = {73, 65, 70, 0};

#region CharacterMap
    //
    // There are 42 equivalence classes
    // There are 2 character sequence regions
    // There are 1 tables, 127 entries
    // There are 1 runs, 0 singletons
    //
    static sbyte[] map0 = new sbyte[127] {
/* \0     */ 10, 10, 10, 10, 10, 10, 10, 10, 10, 41, 0, 41, 41, 41, 10, 10, 
/* \020   */ 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 
/* \040   */ 41, 28, 8, 40, 12, 35, 10, 11, 24, 25, 34, 27, 37, 7, 5, 33, 
/* 0      */ 1, 4, 4, 4, 4, 4, 4, 4, 4, 4, 13, 38, 31, 26, 30, 39, 
/* @      */ 36, 16, 16, 16, 16, 17, 16, 18, 18, 18, 18, 18, 18, 18, 18, 18, 
/* P      */ 18, 18, 18, 18, 18, 18, 18, 18, 15, 18, 18, 20, 9, 21, 10, 14, 
/* `      */ 10, 3, 3, 3, 3, 6, 3, 19, 19, 19, 19, 19, 19, 19, 19, 19, 
/* p      */ 19, 19, 19, 19, 19, 19, 19, 19, 2, 19, 19, 22, 32, 23, 29 };

    sbyte Map(int chr)
    { // '\0' <= chr <= '\uFFFF'
      if (chr < 127) return map0[chr - 0];
      else return (sbyte)10;
    }
#endregion

    static Table[] NxS = new Table[88];

    static Scanner() {
    NxS[0] = new Table(0, 0, 0, null);
    NxS[1] = new Table(0, 0, -1, null);
    NxS[2] = new Table(1, 17, -1, new sbyte[] {4, 87, -1, 4, 85, 86, 
        -1, -1, -1, -1, -1, -1, -1, -1, 87, -1, 86});
    NxS[3] = new Table(1, 19, -1, new sbyte[] {3, 3, 3, 3, -1, 3, 
        -1, -1, -1, -1, -1, -1, 77, 3, 3, 3, 3, 3, 3});
    NxS[4] = new Table(1, 17, -1, new sbyte[] {4, -1, -1, 4, 85, 86, 
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 86});
    NxS[5] = new Table(0, 0, -1, null);
    NxS[6] = new Table(26, 5, -1, new sbyte[] {60, -1, -1, -1, 61});
    NxS[7] = new Table(0, 10, 83, new sbyte[] {-1, 83, 83, 83, 83, 83, 
        83, 83, 59, 84});
    NxS[8] = new Table(0, 0, -1, null);
    NxS[9] = new Table(0, 12, 81, new sbyte[] {-1, 81, 81, 81, 81, 81, 
        81, 81, 81, 82, 81, 58});
    NxS[10] = new Table(1, 19, -1, new sbyte[] {57, 57, 57, 57, -1, 57, 
        -1, -1, -1, -1, -1, -1, 79, 57, 57, 57, 57, 57, 57});
    NxS[11] = new Table(13, 1, -1, new sbyte[] {76});
    NxS[12] = new Table(1, 19, -1, new sbyte[] {12, 12, 12, 12, -1, 12, 
        -1, -1, -1, -1, -1, -1, 74, 12, 12, 12, 12, 12, 12});
    NxS[13] = new Table(0, 0, -1, null);
    NxS[14] = new Table(0, 0, -1, null);
    NxS[15] = new Table(0, 0, -1, null);
    NxS[16] = new Table(0, 0, -1, null);
    NxS[17] = new Table(0, 0, -1, null);
    NxS[18] = new Table(0, 0, -1, null);
    NxS[19] = new Table(26, 5, -1, new sbyte[] {54, -1, -1, 55, 56});
    NxS[20] = new Table(26, 5, -1, new sbyte[] {52, -1, -1, -1, 53});
    NxS[21] = new Table(26, 4, -1, new sbyte[] {50, -1, -1, 51});
    NxS[22] = new Table(30, 1, -1, new sbyte[] {49});
    NxS[23] = new Table(26, 5, -1, new sbyte[] {47, -1, -1, -1, 48});
    NxS[24] = new Table(26, 24, -1, new sbyte[] {42, -1, -1, 43, -1, 44, 
        45, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
        -1, 41});
    NxS[25] = new Table(30, 1, -1, new sbyte[] {39});
    NxS[26] = new Table(34, 1, -1, new sbyte[] {36});
    NxS[27] = new Table(0, 0, -1, null);
    NxS[28] = new Table(0, 0, -1, null);
    NxS[29] = new Table(36, 1, -1, new sbyte[] {35});
    NxS[30] = new Table(0, 0, -1, null);
    NxS[31] = new Table(0, 0, -1, null);
    NxS[32] = new Table(0, 0, -1, null);
    NxS[33] = new Table(0, 0, -1, null);
    NxS[34] = new Table(41, 1, -1, new sbyte[] {34});
    NxS[35] = new Table(0, 0, -1, null);
    NxS[36] = new Table(34, 9, 36, new sbyte[] {37, 36, 36, 36, 36, 36, 
        36, 36, -1});
    NxS[37] = new Table(33, 2, -1, new sbyte[] {38, 37});
    NxS[38] = new Table(0, 0, -1, null);
    NxS[39] = new Table(30, 1, -1, new sbyte[] {40});
    NxS[40] = new Table(0, 0, -1, null);
    NxS[41] = new Table(0, 0, -1, null);
    NxS[42] = new Table(0, 0, -1, null);
    NxS[43] = new Table(0, 0, -1, null);
    NxS[44] = new Table(32, 1, -1, new sbyte[] {46});
    NxS[45] = new Table(0, 0, -1, null);
    NxS[46] = new Table(0, 0, -1, null);
    NxS[47] = new Table(0, 0, -1, null);
    NxS[48] = new Table(0, 0, -1, null);
    NxS[49] = new Table(0, 0, -1, null);
    NxS[50] = new Table(0, 0, -1, null);
    NxS[51] = new Table(0, 0, -1, null);
    NxS[52] = new Table(0, 0, -1, null);
    NxS[53] = new Table(0, 0, -1, null);
    NxS[54] = new Table(0, 0, -1, null);
    NxS[55] = new Table(0, 0, -1, null);
    NxS[56] = new Table(0, 0, -1, null);
    NxS[57] = new Table(1, 19, -1, new sbyte[] {57, 57, 57, 57, -1, 57, 
        -1, -1, -1, -1, -1, -1, 79, 57, 57, 57, 57, 57, 57});
    NxS[58] = new Table(0, 0, -1, null);
    NxS[59] = new Table(0, 0, -1, null);
    NxS[60] = new Table(0, 0, -1, null);
    NxS[61] = new Table(0, 0, -1, null);
    NxS[62] = new Table(1, 4, -1, new sbyte[] {62, -1, -1, 62});
    NxS[63] = new Table(1, 17, -1, new sbyte[] {63, -1, -1, 63, -1, 86, 
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 86});
    NxS[64] = new Table(1, 17, -1, new sbyte[] {64, -1, 64, 64, -1, 64, 
        -1, -1, -1, -1, -1, -1, -1, -1, -1, 64, 64});
    NxS[65] = new Table(34, 9, 67, new sbyte[] {68, 67, 67, 67, 67, 67, 
        67, 67, 66});
    NxS[66] = new Table(0, 0, -1, null);
    NxS[67] = new Table(34, 9, 67, new sbyte[] {68, 67, 67, 67, 67, 67, 
        67, 67, -1});
    NxS[68] = new Table(33, 2, -1, new sbyte[] {69, 68});
    NxS[69] = new Table(0, 0, -1, null);
    NxS[70] = new Table(0, 1, 72, new sbyte[] {71});
    NxS[71] = new Table(0, 0, -1, null);
    NxS[72] = new Table(0, 1, 72, new sbyte[] {-1});
    NxS[73] = new Table(19, 38, 12, new sbyte[] {3, 13, 14, 15, 16, 17, 
        18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 
        34, 1, 2, 3, 3, 4, 5, 3, 6, 7, 8, 8, 9, 10, 11, 8});
    NxS[74] = new Table(13, 1, -1, new sbyte[] {75});
    NxS[75] = new Table(15, 4, -1, new sbyte[] {12, 12, 12, 12});
    NxS[76] = new Table(2, 18, -1, new sbyte[] {3, 3, -1, -1, 3, -1, 
        -1, -1, -1, -1, -1, -1, -1, 12, 12, 12, 12, 3});
    NxS[77] = new Table(13, 1, -1, new sbyte[] {78});
    NxS[78] = new Table(2, 18, -1, new sbyte[] {3, 3, -1, -1, 3, -1, 
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3});
    NxS[79] = new Table(13, 1, -1, new sbyte[] {80});
    NxS[80] = new Table(1, 19, -1, new sbyte[] {57, 57, 57, 57, -1, 57, 
        -1, -1, -1, -1, -1, -1, -1, 57, 57, 57, 57, 57, 57});
    NxS[81] = new Table(0, 12, 81, new sbyte[] {-1, 81, 81, 81, 81, 81, 
        81, 81, 81, 82, 81, 58});
    NxS[82] = new Table(0, 1, 81, new sbyte[] {-1});
    NxS[83] = new Table(0, 10, 83, new sbyte[] {-1, 83, 83, 83, 83, 83, 
        83, 83, 59, 84});
    NxS[84] = new Table(0, 1, 83, new sbyte[] {-1});
    NxS[85] = new Table(1, 4, -1, new sbyte[] {63, -1, -1, 63});
    NxS[86] = new Table(1, 7, -1, new sbyte[] {62, -1, -1, 62, -1, -1, 
        85});
    NxS[87] = new Table(1, 17, -1, new sbyte[] {64, -1, 64, 64, -1, 64, 
        -1, -1, -1, -1, -1, -1, -1, -1, -1, 64, 64});
    }

int NextState(int qStat) {
    if (chr == ScanBuff.EOF)
        return (qStat <= maxAccept && qStat != currentStart ? currentStart : eofNum);
    else {
        int rslt;
        int idx = Map(chr) - NxS[qStat].min;
        if (idx < 0) idx += 42;
        if ((uint)idx >= (uint)NxS[qStat].rng) rslt = NxS[qStat].dflt;
        else rslt = NxS[qStat].nxt[idx];
        return (rslt == goStart ? currentStart : rslt);
    }
}

int NextState() {
    if (chr == ScanBuff.EOF)
        return (state <= maxAccept && state != currentStart ? currentStart : eofNum);
    else {
        int rslt;
        int idx = Map(chr) - NxS[state].min;
        if (idx < 0) idx += 42;
        if ((uint)idx >= (uint)NxS[state].rng) rslt = NxS[state].dflt;
        else rslt = NxS[state].nxt[idx];
        return (rslt == goStart ? currentStart : rslt);
    }
}
#endregion


#if BACKUP
        // ====================== Nested class ==========================

        internal class Context // class used for automaton backup.
        {
            public int bPos;
            public int cNum;
            public int state;
            public int cChr;
        }
#endif // BACKUP


        // ====================== Nested class ==========================

        public sealed class StringBuff : ScanBuff
        {
            string str;        // input buffer
            int bPos;          // current position in buffer
            int sLen;

            public StringBuff(string str)
            {
                this.str = str;
                this.sLen = str.Length;
            }

            public override int Read()
            {
                if (bPos < sLen) return str[bPos++];
                else if (bPos == sLen) { bPos++; return '\n'; }   // one strike, see newline
                else return EOF;                                  // two strikes and you're out!
            }
            
            public override int ReadPos { get { return bPos - 1; } }

            public override int Peek()
            {
                if (bPos < sLen) return str[bPos];
                else return '\n';
            }

            public override string GetString(int beg, int end)
            {
                //  "end" can be greater than sLen with the BABEL
                //  option set.  Read returns a "virtual" EOL if
                //  an attempt is made to read past the end of the
                //  string buffer.  Without the guard any attempt 
                //  to fetch yytext for a token that includes the 
                //  EOL will throw an index exception.
                if (end > sLen) end = sLen;
                if (end <= beg) return ""; 
                else return str.Substring(beg, end - beg);
            }

            public override int Pos
            {
                get { return bPos; }
                set { bPos = value; }
            }
        }

        // ====================== Nested class ==========================

        public sealed class StreamBuff : ScanBuff
        {
            BufferedStream bStrm;   // input buffer
            int delta = 1;

            public StreamBuff(Stream str) { this.bStrm = new BufferedStream(str); }

            public override int Read() {
                return bStrm.ReadByte(); 
            }
            
            public override int ReadPos {
                get { return (int)bStrm.Position - delta; }
            }

            public override int Peek()
            {
                int rslt = bStrm.ReadByte();
                bStrm.Seek(-delta, SeekOrigin.Current);
                return rslt;
            }

            public override string GetString(int beg, int end)
            {
                if (end - beg <= 0) return "";
                long savePos = bStrm.Position;
                char[] arr = new char[end - beg];
                bStrm.Position = (long)beg;
                for (int i = 0; i < (end - beg); i++)
                    arr[i] = (char)bStrm.ReadByte();
                bStrm.Position = savePos;
                return new String(arr);
            }

            // Pos is the position *after* reading chr!
            public override int Pos
            {
                get { return (int)bStrm.Position; }
                set { bStrm.Position = value; }
            }
        }

        // ====================== Nested class ==========================

        /// <summary>
        /// This is the Buffer for UTF8 files.
        /// It attempts to read the encoding preamble, which for 
        /// this encoding should be unicode point \uFEFF which is 
        /// encoded as EF BB BF
        /// </summary>
        public class TextBuff : ScanBuff
        {
            protected BufferedStream bStrm;   // input buffer
            protected int delta = 1;
            
            private Exception BadUTF8()
            { return new Exception(String.Format("BadUTF8 Character")); }

            /// <summary>
            /// TextBuff factory.  Reads the file preamble
            /// and returns a TextBuff, LittleEndTextBuff or
            /// BigEndTextBuff according to the result.
            /// </summary>
            /// <param name="strm">The underlying stream</param>
            /// <returns></returns>
            public static TextBuff NewTextBuff(Stream strm)
            {
                // First check if this is a UTF16 file
                //
                int b0 = strm.ReadByte();
                int b1 = strm.ReadByte();

                if (b0 == 0xfe && b1 == 0xff)
                    return new BigEndTextBuff(strm);
                if (b0 == 0xff && b1 == 0xfe)
                    return new LittleEndTextBuff(strm);
                
                int b2 = strm.ReadByte();
                if (b0 == 0xef && b1 == 0xbb && b2 == 0xbf)
                    return new TextBuff(strm);
                //
                // There is no unicode preamble, so we
                // must go back to the UTF8 default.
                //
                strm.Seek(0, SeekOrigin.Begin);
                return new TextBuff(strm);
            }

            protected TextBuff(Stream str) { 
                this.bStrm = new BufferedStream(str);
            }

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1;
                int ch2;
                if (ch0 < 0x7f)
                {
                    delta = (ch0 == EOF ? 0 : 1);
                    return ch0;
                }
                else if ((ch0 & 0xe0) == 0xc0)
                {
                    delta = 2;
                    ch1 = bStrm.ReadByte();
                    if ((ch1 & 0xc0) == 0x80)
                        return ((ch0 & 0x1f) << 6) + (ch1 & 0x3f);
                    else
                        throw BadUTF8();
                }
                else if ((ch0 & 0xf0) == 0xe0)
                {
                    delta = 3;
                    ch1 = bStrm.ReadByte();
                    ch2 = bStrm.ReadByte();
                    if ((ch1 & ch2 & 0xc0) == 0x80)
                        return ((ch0 & 0xf) << 12) + ((ch1 & 0x3f) << 6) + (ch2 & 0x3f);
                    else
                        throw BadUTF8();
                }
                else
                    throw BadUTF8();
            }

            public sealed override int ReadPos
            {
                get { return (int)bStrm.Position - delta; }
            }

            public sealed override int Peek()
            {
                int rslt = Read();
                bStrm.Seek(-delta, SeekOrigin.Current);
                return rslt;
            }

            /// <summary>
            /// Returns the string from the buffer between
            /// the given file positions.  This needs to be
            /// done carefully, as the number of characters
            /// is, in general, not equal to (end - beg).
            /// </summary>
            /// <param name="beg">Begin filepos</param>
            /// <param name="end">End filepos</param>
            /// <returns></returns>
            public sealed override string GetString(int beg, int end)
            {
                int i;
                if (end - beg <= 0) return "";
                long savePos = bStrm.Position;
                char[] arr = new char[end - beg];
                bStrm.Position = (long)beg;
                for (i = 0; bStrm.Position < end; i++)
                    arr[i] = (char)Read();
                bStrm.Position = savePos;
                return new String(arr, 0, i);
            }

            // Pos is the position *after* reading chr!
            public sealed override int Pos
            {
                get { return (int)bStrm.Position; }
                set { bStrm.Position = value; }
            }
        }

        // ====================== Nested class ==========================
        /// <summary>
        /// This is the Buffer for Big-endian UTF16 files.
        /// </summary>
        public sealed class BigEndTextBuff : TextBuff
        {
            internal BigEndTextBuff(Stream str) : base(str) { } // 

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1 = bStrm.ReadByte();
                return (ch0 << 8) + ch1;
            }
        }
        
        // ====================== Nested class ==========================
        /// <summary>
        /// This is the Buffer for Little-endian UTF16 files.
        /// </summary>
        public sealed class LittleEndTextBuff : TextBuff
        {
            internal LittleEndTextBuff(Stream str) : base(str) { } // { this.bStrm = new BufferedStream(str); }

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1 = bStrm.ReadByte();
                return (ch1 << 8) + ch0;
            }
        }
        
        // =================== End Nested classes =======================

        public Scanner(Stream file) {
            buffer = TextBuff.NewTextBuff(file); // selected by /unicode option
            this.cNum = -1;
            this.chr = '\n'; // to initialize yyline, yycol and lineStart
            GetChr();
        }

        public Scanner() { }

        void GetChr()
        {
            if (chr == '\n') 
            { 
                lineStartNum = cNum + 1; 
                lNum++; 
            }
            chr = buffer.Read();
            cNum++;
        }

        void MarkToken()
        {
            tokPos = buffer.ReadPos;
            tokNum = cNum;
            tokLin = lNum;
            tokCol = cNum - lineStartNum;
        }
        
        void MarkEnd()
        {
            tokTxt = null;
            tokLen = cNum - tokNum;
            tokEPos = buffer.ReadPos;
            tokELin = lNum;
            tokECol = cNum - lineStartNum;
        }
 
        // ================ StringBuffer Initialization ===================

        public void SetSource(string source, int offset)
        {
            this.buffer = new StringBuff(source);
            this.buffer.Pos = offset;
            this.cNum = offset - 1;
            this.chr = '\n'; // to initialize yyline, yycol and lineStart
            GetChr();
        }
        
        public int GetNext(ref int state, out int start, out int end)
        {
            Tokens next;
            EolState = state;
            next = (Tokens)Scan();
            state = EolState;
            start = tokPos;
            end = tokEPos - 1; // end is the index of last char.
            return (int)next;
        }

        // ======== IScanner<> Implementation =========

        public override int yylex()
        {
            // parserMax is set by reflecting on the Tokens
            // enumeration.  If maxParseTokeen is defined
            // that is used, otherwise int.MaxValue is used.
            //
            int next;
            do { next = Scan(); } while (next >= parserMax);
            return next;
        }
        
        int yyleng { get { return tokLen; } }
        int yypos { get { return tokPos; } }
        int yyline { get { return tokLin; } }
        int yycol { get { return tokCol; } }

        public string yytext
        {
            get 
            {
                if (tokTxt == null) 
                    tokTxt = buffer.GetString(tokPos, tokEPos);
                return tokTxt;
            }
        }

        void yyless(int n) { 
            buffer.Pos = tokPos;
            cNum = tokNum;
            for (int i = 0; i <= n; i++) GetChr();
            MarkEnd();
        }

        public IErrorHandler Handler { get { return this.handler; }
                                       set { this.handler = value; }}

        // ============ methods available in actions ==============

        internal int YY_START {
            get { return CurrentSc; }
            set { CurrentSc = value; } 
        }

        // ============== The main tokenizer code =================

        int Scan()
        {
            try {
                for (; ; )
                {
                    int next;              // next state to enter                   
#if BACKUP
                    bool inAccept = false; // inAccept ==> current state is an accept state
                    Result rslt = Result.noMatch;
                    // skip "idle" transitions
#if LEFTANCHORS
                    if (lineStartNum == cNum && NextState(anchorState[CurrentSc]) != currentStart)
                        state = anchorState[CurrentSc];
                    else {
                        state = currentStart;
                        while (NextState() == state) {
                            GetChr();
                            if (lineStartNum == cNum) {
                                int anchor = anchorState[CurrentSc];
                                if (NextState(anchor) != state) {
                                    state = anchor; 
                                    break;
                                }
                            }
                        }
                    }
#else // !LEFTANCHORS
                    state = currentStart;
                    while (NextState() == state) 
                        GetChr(); // skip "idle" transitions
#endif // LEFTANCHORS
                    MarkToken();
                    
                    while ((next = NextState()) != currentStart)
                        if (inAccept && next > maxAccept) // need to prepare backup data
                        {
                            Context ctx = new Context();
                            rslt = Recurse2(ctx, next);
                            if (rslt == Result.noMatch) RestoreStateAndPos(ctx);
                            // else if (rslt == Result.contextFound) RestorePos(ctx);
                            break;
                        }
                        else
                        {
                            state = next;
                            GetChr();
                            if (state <= maxAccept) inAccept = true;
                        }
#else // !BACKUP
#if LEFTANCHORS
                    if (lineStartNum == cNum) {
                        int anchor = anchorState[CurrentSc];
                        if (NextState(anchor) != currentStart)
                            state = anchor;
                    }
                    else {
                        state = currentStart;
                        while (NextState() == state) {
                            GetChr();
                            if (lineStartNum == cNum) {
                                anchor = anchorState[CurrentSc];
                                if (NextState(anchor) != state) {
                                    state = anchor;
                                    break;
                                }
                            }
                        }
                    }
#else // !LEFTANCHORS
                    state = currentStart;
                    while (NextState() == state) 
                        GetChr(); // skip "idle" transitions
#endif // LEFTANCHORS
                    MarkToken();
                    // common code
                    while ((next = NextState()) != currentStart)
                    {
                        state = next;
                        GetChr();
                    }
#endif // BACKUP
                    if (state > maxAccept) 
                        state = currentStart;
                    else
                    {
                        MarkEnd();
#region ActionSwitch
#pragma warning disable 162
    switch (state)
    {
        case eofNum:
            return (int)Tokens.EOF;
        case 1:
return (int)Tokens.LEX_WHITE;
            break;
        case 2:
        case 4:
        case 62:
        case 63:
        case 64:
return GetTokenId("number");
            break;
        case 3:
return GetKeywordOrName(yytext);
            break;
        case 5:
return GetTokenId(yytext);
            break;
        case 6:
return GetTokenId(yytext);
            break;
        case 7:
        case 8:
        case 9:
        case 10:
yyerror("illegal char"); return (int)Tokens.LEX_ERROR;
            break;
        case 11:
return GetTokenId(yytext);
            break;
        case 12:
return GetTokenId("classref");
            break;
        case 13:
return GetTokenId(yytext);
            break;
        case 14:
return GetTokenId(yytext);
            break;
        case 15:
return GetTokenId(yytext);
            break;
        case 16:
return GetTokenId(yytext);
            break;
        case 17:
return GetTokenId(yytext);
            break;
        case 18:
return GetTokenId(yytext);
            break;
        case 19:
return GetTokenId(yytext);
            break;
        case 20:
return GetTokenId(yytext);
            break;
        case 21:
return GetTokenId(yytext);
            break;
        case 22:
return GetTokenId(yytext);
            break;
        case 23:
return GetTokenId(yytext);
            break;
        case 24:
return GetTokenId(yytext);
            break;
        case 25:
return GetTokenId(yytext);
            break;
        case 26:
return GetDivideOrRegex(yytext);
            break;
        case 27:
return GetTokenId(yytext);
            break;
        case 28:
return GetTokenId(yytext);
            break;
        case 29:
return GetTokenId(yytext);
            break;
        case 30:
return GetTokenId(yytext);
            break;
        case 31:
return GetTokenId(yytext);
            break;
        case 32:
return GetTokenId(yytext);
            break;
        case 33:
BEGIN(LN_COMMENT); return GetTokenId("line.comment");
            break;
        case 34:
return (int)Tokens.LEX_WHITE;
            break;
        case 35:
return GetTokenId(yytext);
            break;
        case 36:
        case 37:
BEGIN(COMMENT); return GetTokenId("block.comment");
            break;
        case 38:
return GetTokenId("block.comment");
            break;
        case 39:
return GetTokenId(yytext);
            break;
        case 40:
return GetTokenId(yytext);
            break;
        case 41:
return GetTokenId(yytext);
            break;
        case 42:
return GetTokenId(yytext);
            break;
        case 43:
return GetTokenId(yytext);
            break;
        case 44:
return GetTokenId(yytext);
            break;
        case 45:
return GetTokenId(yytext);
            break;
        case 46:
return GetTokenId(yytext);
            break;
        case 47:
return GetTokenId(yytext);
            break;
        case 48:
return GetTokenId(yytext);
            break;
        case 49:
return GetTokenId(yytext);
            break;
        case 50:
return GetTokenId(yytext);
            break;
        case 51:
return GetTokenId(yytext);
            break;
        case 52:
return GetTokenId(yytext);
            break;
        case 53:
return GetTokenId(yytext);
            break;
        case 54:
return GetTokenId(yytext);
            break;
        case 55:
return GetTokenId(yytext);
            break;
        case 56:
return GetTokenId(yytext);
            break;
        case 57:
return GetTokenId("variable");
            break;
        case 58:
return GetTokenId("string");
            break;
        case 59:
return GetTokenId("string");
            break;
        case 60:
return GetTokenId(yytext);
            break;
        case 61:
return GetTokenId(yytext);
            break;
        case 65:
        case 66:
        case 67:
        case 68:
return GetTokenId("block.comment");
            break;
        case 69:
BEGIN(INITIAL); return GetTokenId("block.comment");
            break;
        case 70:
        case 72:
return GetTokenId("line.comment");
            break;
        case 71:
BEGIN (INITIAL);
            break;
        default:
            break;
    }
#pragma warning restore 162
#endregion
                    }
                }
            } // end try
            finally {
LoadYylval();
            } // end finally
        }

#if BACKUP
        Result Recurse2(Context ctx, int next)
        {
            // Assert: at entry "state" is an accept state AND
            //         NextState(state, chr) != currentStart AND
            //         NextState(state, chr) is not an accept state.
            //
            bool inAccept;
            SaveStateAndPos(ctx);
            state = next;
            if (state == eofNum) return Result.accept;
            GetChr();
            inAccept = false;

            while ((next = NextState()) != currentStart)
            {
                if (inAccept && next > maxAccept) // need to prepare backup data
                    SaveStateAndPos(ctx);
                state = next;
                if (state == eofNum) return Result.accept;
                GetChr(); 
                inAccept = (state <= maxAccept);
            }
            if (inAccept) return Result.accept; else return Result.noMatch;
        }

        void SaveStateAndPos(Context ctx)
        {
            ctx.bPos  = buffer.Pos;
            ctx.cNum  = cNum;
            ctx.state = state;
            ctx.cChr = chr;
        }

        void RestoreStateAndPos(Context ctx)
        {
            buffer.Pos = ctx.bPos;
            cNum = ctx.cNum;
            state = ctx.state;
            chr = ctx.cChr;
        }

        void RestorePos(Context ctx) { buffer.Pos = ctx.bPos; cNum = ctx.cNum; }
#endif // BACKUP

        // ============= End of the tokenizer code ================

        internal void BEGIN(int next)
        { CurrentSc = next; }

#if STACK        
        internal void yy_clear_stack() { scStack.Clear(); }
        internal int yy_top_state() { return scStack.Peek(); }
        
        internal void yy_push_state(int state)
        {
            scStack.Push(CurrentSc);
            CurrentSc = state;
        }
        
        internal void yy_pop_state()
        {
            // Protect against input errors that pop too far ...
            if (scStack.Count > 0) {
				int newSc = scStack.Pop();
				CurrentSc = newSc;
            } // Otherwise leave stack unchanged.
        }
 #endif // STACK

        internal void ECHO() { Console.Out.Write(yytext); }
        
#region UserCodeSection

/* .... */

#endregion
    } // end class Scanner
} // end namespace

%namespace Puppet.Lexer
%using Puppet;
%using Puppet.Parser;
%using System.Text;
%using System.Diagnostics;
%using Microsoft.VisualStudio.Package;



%x COMMENT
%x LN_COMMENT


%{
        //form lexer.lex

        public class TokDesc
        {

            public TokDesc()
            {
                this.Value = "undefined";
                this.TokenType = TokenType.Unknown;
                this.Remark = string.Empty;
            }

            public TokDesc(string value, TokenType tokenType, string remark)
            {
                this.Value = value;
                this.TokenType = tokenType;
                this.Remark = remark;
            }
            
            public string Value{get; private set; }
            public TokenType TokenType{get; private set; }
            public string Remark{get; private set; }
        }
        
        private class TokDescHandler<TOK_DESC>
        {
            private Dictionary<Tokens, TOK_DESC> tokenToDescMap;

            private Dictionary<Tokens, TOK_DESC> TokenToDescMap
            {
                get
                {
                    if (null == this.tokenToDescMap)
                    {
                        this.tokenToDescMap = new Dictionary<Tokens, TOK_DESC>();
                    }

                    return this.tokenToDescMap;
                }
            }
            
            public int AddTokDesc(Tokens token, TOK_DESC desc)
            {
                if (!this.TokenToDescMap.ContainsKey(token))
                {
                    this.TokenToDescMap.Add(token, desc);
                }

                return (int)token;
            }
            
            public TOK_DESC GetTokDesc(Tokens token)
            {
                if (this.TokenToDescMap.ContainsKey(token))
                {
                    return this.TokenToDescMap[token];
                }

                return default(TOK_DESC);
            }
        }

        private static TokDescHandler<TokDesc> tokDescHandler = new TokDescHandler<TokDesc>();

        public static TokDescHandler<TokDesc> TokDescHandler { get { return Scanner.tokDescHandler; } }

        private static int AddTokDesc(Tokens token, string value, TokenType tokenType, string remark)
        {
            Scanner.tokDescHandler.AddTokDesc(token, new TokDesc(value, tokenType, remark));
            
            return (int)token;
        }

        public static TokDesc GetTokDesc(Tokens token)
        {
            return Scanner.tokDescHandler.GetTokDesc(token);
        }

        private static Dictionary<string,Tokens> keywords;

        public static Dictionary<string,Tokens> Keywords
        {
            get
            {
                if(null == Scanner.keywords)
                {
                    Scanner.keywords = new Dictionary<string,Tokens>
                    {            
                        { "case"            , Tokens.CASE        },
                        { "class"           , Tokens.CLASS        },
                        { "default"         , Tokens.DEFAULT    },
                        { "define"          , Tokens.DEFINE    },
                        { "if"              , Tokens.IF        },
                        { "elsif"           , Tokens.ELSIF        },
                        { "else"            , Tokens.ELSE        },
                        { "inherits"        , Tokens.INHERITS    },
                        { "node"            , Tokens.NODE        },
                        { "and"             , Tokens.AND        },
                        { "or"              , Tokens.OR        },
                        { "undef"           , Tokens.UNDEF        },
                        { "false"           , Tokens.BOOLEAN    },
                        { "true"            , Tokens.BOOLEAN    },
                        { "in"              , Tokens.IN        },
                        { "unless"          , Tokens.UNLESS    },
                    };
                }

                return Scanner.keywords;
            }
        }

        static int GetIdToken(string txt)
        {
            if(txt.StartsWith("::"))
            {
                return Scanner.AddTokDesc(Tokens.NAME, txt, TokenType.Identifier, "name");
            }

            if(Keywords.ContainsKey(txt))
            {
                return Scanner.AddTokDesc(keywords[txt], txt, TokenType.Keyword, txt);
            }

            return AddTokDesc(Tokens.NAME, txt, TokenType.Identifier, "name");
        }
        
        int GetDivideOrRegex(string txt)
        {
            var ctx = new Context();
            SaveStateAndPos(ctx);
            int pch = txt[0];
            int ch;
            while( (ch = this.buffer.Read()) != '\n' && ch != ScanBuff.EOF)
            {
                if ((char)ch == '/' && (char)pch != '\\')
                {
                    GetChr();
                    this.MarkEnd();
                    return Scanner.AddTokDesc(Tokens.REGEX, txt, TokenType.Literal, "regex");
               }

                pch = ch;
            }

            RestoreStateAndPos(ctx);
            return Scanner.AddTokDesc(Tokens.DIV, txt, TokenType.Operator, "div");
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
%}


White0                  [\t\r\f\v ]
White                   {White0}|\n

CmntStart               \/\*
CmntEnd                 \*\/
ABStar                  [^\*\n]*

DQSStart                \"
DQNewLine               [^\n]*
DQSEnd                  \"

ClassRef                ((::)?[A-Z][A-Za-z0-9_]*)+
Name                    ((::)?[a-z][A-Za-z0-9_]*)(::[a-z][A-Za-z0-9_]*)*

DollarVar               \$(::)?([A-Za-z0-9_]+::)*[A-Za-z0-9_]+
Number                  (0[xX][0-9A-Fa-f]+|0?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+)?)

Regex                   \/[^\*](\\.|[^\\/])*\/
DQString                \"(\\.|[^\\"\n])*\"
SQString                \'(\\.|[^\\\'\n])*\'


%%

{Number}                    { return AddTokDesc(Tokens.NUMBER, yytext, TokenType.Literal, "number"); }
{DQString}                  { return AddTokDesc(Tokens.STRING, yytext, TokenType.Literal, "string"); }
{SQString}                  { return AddTokDesc(Tokens.STRING, yytext, TokenType.Literal, "string"); }
{DollarVar}                 { return AddTokDesc(Tokens.VARIABLE, yytext, TokenType.Identifier, "variable"); }
{ClassRef}                  { return AddTokDesc(Tokens.CLASSREF, yytext, TokenType.Identifier, "class reference"); }
{Name}                      { return GetIdToken(yytext); }
"["                         { return AddTokDesc(Tokens.LBRACK, yytext, TokenType.Delimiter, yytext); }
"]"                         { return AddTokDesc(Tokens.RBRACK, yytext, TokenType.Delimiter, yytext); }
"{"                         { return AddTokDesc(Tokens.LBRACE, yytext, TokenType.Delimiter, yytext); }
"}"                         { return AddTokDesc(Tokens.RBRACE, yytext, TokenType.Delimiter, yytext); }
"("                         { return AddTokDesc(Tokens.LPAREN, yytext, TokenType.Delimiter, yytext); }
")"                         { return AddTokDesc(Tokens.RPAREN, yytext, TokenType.Delimiter, yytext); }
=                           { return AddTokDesc(Tokens.EQUALS, yytext, TokenType.Operator, yytext); }
"+="                        { return AddTokDesc(Tokens.APPENDS, yytext, TokenType.Operator, yytext); }
\-=                         { return AddTokDesc(Tokens.DELETES, yytext, TokenType.Operator, yytext); }
==                          { return AddTokDesc(Tokens.ISEQUAL, yytext, TokenType.Operator, yytext); }
\!=                         { return AddTokDesc(Tokens.NOTEQUAL, yytext, TokenType.Operator, yytext); }
=\~                         { return AddTokDesc(Tokens.MATCH, yytext, TokenType.Operator, yytext); }
\!~                         { return AddTokDesc(Tokens.NOMATCH, yytext, TokenType.Operator, yytext); }
\>=                         { return AddTokDesc(Tokens.GREATEREQUAL, yytext, TokenType.Operator, yytext); }
\>                          { return AddTokDesc(Tokens.GREATERTHAN, yytext, TokenType.Operator, yytext); }
\<=                         { return AddTokDesc(Tokens.LESSEQUAL, yytext, TokenType.Operator, yytext); }
\<                          { return AddTokDesc(Tokens.LESSTHAN, yytext, TokenType.Operator, yytext); }
=\>                         { return AddTokDesc(Tokens.FARROW, yytext, TokenType.Operator, yytext); }
\+\>                        { return AddTokDesc(Tokens.PARROW, yytext, TokenType.Operator, yytext); }
\<\<                        { return AddTokDesc(Tokens.LSHIFT, yytext, TokenType.Operator, yytext); }
\<\<\|                      { return AddTokDesc(Tokens.LLCOLLECT, yytext, TokenType.Operator, yytext); }
\<\|                        { return AddTokDesc(Tokens.LCOLLECT, yytext, TokenType.Operator, yytext); }
\>\>                        { return AddTokDesc(Tokens.RSHIFT, yytext, TokenType.Operator, yytext); }
\|\>\>                      { return AddTokDesc(Tokens.RRCOLLECT, yytext, TokenType.Operator, yytext); }
\|\>                        { return AddTokDesc(Tokens.RCOLLECT, yytext, TokenType.Operator, yytext); }
\+                          { return AddTokDesc(Tokens.PLUS, yytext, TokenType.Operator, yytext); }
\-                          { return AddTokDesc(Tokens.MINUS, yytext, TokenType.Operator, yytext); }
\/                          { return GetDivideOrRegex(yytext); }
\*                          { return AddTokDesc(Tokens.TIMES, yytext, TokenType.Operator, yytext); }
\%                          { return AddTokDesc(Tokens.MODULO, yytext, TokenType.Operator, yytext); }
\!                          { return AddTokDesc(Tokens.NOT, yytext, TokenType.Operator, yytext); }
\.                          { return AddTokDesc(Tokens.DOT, yytext, TokenType.Operator, yytext); }
\|                          { return AddTokDesc(Tokens.PIPE, yytext, TokenType.Operator, yytext); }
\@                          { return AddTokDesc(Tokens.AT, yytext, TokenType.Operator, yytext); }
\@\@                        { return AddTokDesc(Tokens.ATAT, yytext, TokenType.Operator, yytext); }
:                           { return AddTokDesc(Tokens.COLON, yytext, TokenType.Operator, yytext); }
,                           { return AddTokDesc(Tokens.COMMA, yytext, TokenType.Operator, yytext); }
;                           { return AddTokDesc(Tokens.SEMIC, yytext, TokenType.Operator, yytext); }
\?                          { return AddTokDesc(Tokens.QMARK, yytext, TokenType.Operator, yytext); }
\~                          { return AddTokDesc(Tokens.TILDE, yytext, TokenType.Operator, yytext); }
\-\>                        { return AddTokDesc(Tokens.IN_EDGE, yytext, TokenType.Operator, yytext); }
~\>                         { return AddTokDesc(Tokens.IN_EDGE_SUB, yytext, TokenType.Operator, yytext); }
\<\-                        { return AddTokDesc(Tokens.OUT_EDGE, yytext, TokenType.Operator, yytext); }
\<\~                        { return AddTokDesc(Tokens.OUT_EDGE_SUB, yytext, TokenType.Operator, yytext); }

#                                           { BEGIN(LN_COMMENT); return AddTokDesc(Tokens.LN_COMMENT, yytext, TokenType.LineComment, "line comment"); }
<LN_COMMENT>\n                              { BEGIN (INITIAL); }
<LN_COMMENT>(\\.|[^\n])*                    { return AddTokDesc(Tokens.LN_COMMENT, yytext, TokenType.LineComment, "line comment"); }


{CmntStart}{ABStar}\**{CmntEnd}             { return AddTokDesc(Tokens.BL_COMMENT, yytext, TokenType.Comment, "block comment"); } 
{CmntStart}{ABStar}\**                      { BEGIN(COMMENT); return AddTokDesc(Tokens.BL_COMMENT, yytext, TokenType.Comment, "block comment"); }
<COMMENT>\n                                 |
<COMMENT>{ABStar}\**                        { return AddTokDesc(Tokens.BL_COMMENT, yytext, TokenType.Comment, "block comment"); }
<COMMENT>{ABStar}\**{CmntEnd}               { BEGIN(INITIAL); return AddTokDesc(Tokens.BL_COMMENT, yytext, TokenType.Comment, "block comment"); }


{White0}+                        { return (int)Tokens.LEX_WHITE; }
\n                               { return (int)Tokens.LEX_WHITE; }
.                                { yyerror("illegal char"); return (int)Tokens.LEX_ERROR; }

%{
    LoadYylval();
%}

%%

/* .... */

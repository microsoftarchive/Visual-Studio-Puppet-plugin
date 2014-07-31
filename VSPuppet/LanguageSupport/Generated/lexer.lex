%namespace Puppet.Lexer
%using Puppet;
%using Puppet.Parser;
%using System.Text;
%using System.Diagnostics;
%using System.Linq;
%using Microsoft.VisualStudio.Package;



%x COMMENT
%x LN_COMMENT
%x STR


%{
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
            { "true", new TokDesc( Tokens.BOOLEAN, TokenType.Keyword, "boolean" ) },
            { "false", new TokDesc( Tokens.BOOLEAN, TokenType.Keyword, "boolean" ) },
            { "in", new TokDesc( Tokens.IN, TokenType.Keyword, string.Empty ) },
            { "unless", new TokDesc( Tokens.UNLESS, TokenType.Keyword, string.Empty ) },

            { "regex", new TokDesc( Tokens.REGEX, TokenType.Literal, "regular expression" ) },
            { "/", new TokDesc( Tokens.DIV, TokenType.Operator, "divide" ) },

            { "identifier.tokenkey.puppet.shch", new TokDesc(Tokens.NAME, TokenType.Identifier, "identifier") },
            { "number.tokenkey.puppet.shch", new TokDesc(Tokens.NUMBER, TokenType.Literal, "number") },
            { "string.tokenkey.puppet.shch", new TokDesc(Tokens.STRING, TokenType.Literal, "string") },
            { "variable.tokenkey.puppet.shch", new TokDesc(Tokens.VARIABLE, TokenType.Identifier, "variable") }, 
            { "classref.tokenkey.puppet.shch", new TokDesc(Tokens.CLASSREF, TokenType.Identifier, "class reference") },
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
            { "line.comment.tokenkey.puppet.shch", new TokDesc(Tokens.LN_COMMENT, TokenType.LineComment, "line comment") },
            { "block.comment.tokenkey.puppet.shch", new TokDesc(Tokens.BL_COMMENT, TokenType.Comment, "block comment") },
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
%}


White0                  [\t\r\f\v ]
White                   {White0}|\n

CmntStart               \/\*
CmntEnd                 \*\/
ABStar                  [^\*\n]*

ClassRef                ((::)?[A-Z][A-Za-z0-9_]*)+
Name                    ((::)?[a-z][A-Za-z0-9_]*)(::[a-z][A-Za-z0-9_]*)*

DollarVar               \$(::)?([A-Za-z0-9_]+::)*[A-Za-z0-9_]+
Number                  (0[xX][0-9A-Fa-f]+|0?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+)?)

Regex                   \/[^\*](\\.|[^\\/])*\/
SQString                \'(\\.|[^'\n])*\'
DQString                \"(\\.|[^"\n])*\"
DQStrBody               (\\.|[^\n"])*



%%

{Number}                    { return GetTokenId("number.tokenkey.puppet.shch"); }
{SQString}                  { return GetTokenId("string.tokenkey.puppet.shch"); }
{DQString}                  { return GetTokenId("string.tokenkey.puppet.shch"); }
{DollarVar}                 { return GetTokenId("variable.tokenkey.puppet.shch"); }
{ClassRef}                  { return GetTokenId("classref.tokenkey.puppet.shch"); }
{Name}                      { return GetKeywordOrName(yytext); }
"["                         { return GetTokenId(yytext); }
"]"                         { return GetTokenId(yytext); }
"{"                         { return GetTokenId(yytext); }
"}"                         { return GetTokenId(yytext); }
"("                         { return GetTokenId(yytext); }
")"                         { return GetTokenId(yytext); }
=                           { return GetTokenId(yytext); }
"+="                        { return GetTokenId(yytext); }
\-=                         { return GetTokenId(yytext); }
==                          { return GetTokenId(yytext); }
\!=                         { return GetTokenId(yytext); }
=\~                         { return GetTokenId(yytext); }
\!~                         { return GetTokenId(yytext); }
\>=                         { return GetTokenId(yytext); }
\>                          { return GetTokenId(yytext); }
\<=                         { return GetTokenId(yytext); }
\<                          { return GetTokenId(yytext); }
=\>                         { return GetTokenId(yytext); }
\+\>                        { return GetTokenId(yytext); }
\<\<                        { return GetTokenId(yytext); }
\<\<\|                      { return GetTokenId(yytext); }
\<\|                        { return GetTokenId(yytext); }
\>\>                        { return GetTokenId(yytext); }
\|\>\>                      { return GetTokenId(yytext); }
\|\>                        { return GetTokenId(yytext); }
\+                          { return GetTokenId(yytext); }
\-                          { return GetTokenId(yytext); }
\/                          { return GetDivideOrRegex(yytext); }
\*                          { return GetTokenId(yytext); }
\%                          { return GetTokenId(yytext); }
\!                          { return GetTokenId(yytext); }
\.                          { return GetTokenId(yytext); }
\|                          { return GetTokenId(yytext); }
\@                          { return GetTokenId(yytext); }
\@\@                        { return GetTokenId(yytext); }
:                           { return GetTokenId(yytext); }
,                           { return GetTokenId(yytext); }
;                           { return GetTokenId(yytext); }
\?                          { return GetTokenId(yytext); }
\~                          { return GetTokenId(yytext); }
\-\>                        { return GetTokenId(yytext); }
~\>                         { return GetTokenId(yytext); }
\<\-                        { return GetTokenId(yytext); }
\<\~                        { return GetTokenId(yytext); }

#                                           { BEGIN(LN_COMMENT); return GetTokenId("line.comment.tokenkey.puppet.shch"); }
<LN_COMMENT>\n                              { BEGIN (INITIAL); }
<LN_COMMENT>(\\.|[^\n])*                    { return GetTokenId("line.comment.tokenkey.puppet.shch"); }


{CmntStart}{ABStar}\**{CmntEnd}             { return GetTokenId("block.comment.tokenkey.puppet.shch"); } 
{CmntStart}{ABStar}\**                      { BEGIN(COMMENT); return GetTokenId("block.comment.tokenkey.puppet.shch"); }
<COMMENT>\n                                 |
<COMMENT>{ABStar}\**                        { return GetTokenId("block.comment.tokenkey.puppet.shch"); }
<COMMENT>{ABStar}\**{CmntEnd}               { BEGIN(INITIAL); return GetTokenId("block.comment.tokenkey.puppet.shch"); }

["]{DQStrBody}\n?                          { BEGIN(STR); return GetTokenId("string.tokenkey.puppet.shch"); }
<STR>{DQStrBody}["]                        { BEGIN(INITIAL); return GetTokenId("string.tokenkey.puppet.shch");  }
<STR>{DQStrBody}\n                         { return GetTokenId("string.tokenkey.puppet.shch"); }

{White0}+                        { return (int)Tokens.LEX_WHITE; }
\n                               { return (int)Tokens.LEX_WHITE; }
.                                { yyerror("illegal char"); return (int)Tokens.LEX_ERROR; }

%{
    LoadYylval();
%}

%%

/* .... */

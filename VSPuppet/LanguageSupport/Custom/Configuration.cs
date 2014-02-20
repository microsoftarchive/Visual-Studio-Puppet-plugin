/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/
namespace Puppet
{
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Puppet.Parser;

    public static partial class Configuration
    {
        public const string Name = "Puppet Language";
        public const string Extension = ".pp";

        static CommentInfo myCInfo;
        public static CommentInfo MyCommentInfo { get { return myCInfo; } }

        static Configuration()
        {
            myCInfo.BlockEnd = "/*";
            myCInfo.BlockStart = "*/";
            myCInfo.LineStart = "#";
            myCInfo.UseLineComments = true;

            /*
            // default colors - currently, these need to be declared
            CreateColor("Keyword"       , COLORINDEX.CI_BLUE            , COLORINDEX.CI_USERTEXT_BK, false, false);
            CreateColor("Comment", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_BROWN, COLORINDEX.CI_USERTEXT_BK, false, false);
            CreateColor("String", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Puppet classref", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

            TokenColor variable = CreateColor("Puppet Variable", COLORINDEX.CI_PURPLE, COLORINDEX.CI_USERTEXT_BK, false, false);
            TokenColor regexp = CreateColor("Puppet Regexp", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, false, false);
            TokenColor classref = CreateColor("Puppet classref", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK, false, false);

            TokenColor error = CreateColor("Puppet Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, false);
*/
            //
            // map tokens to color classes
            //

            foreach (var kw in Lexer.Scanner.Keywords)
            {
                ColorToken((int)kw.Value, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            }

            //foreach (var op in Lexer.Scanner.TokenToDescMap.Where(td => td.Value.TokenType == TokenType.Operator))
            //{
            //    ColorToken((int) op.Key, TokenType.Operator, TokenColor.Keyword, TokenTriggers.None);
            //}


            ColorToken((int)Tokens.NUMBER       , TokenType.Literal     , TokenColor.Number     , TokenTriggers.None);
            ColorToken((int)Tokens.STRING       , TokenType.String      , TokenColor.String     , TokenTriggers.None);
            ColorToken((int)Tokens.VARIABLE     , TokenType.Identifier  , TokenColor.Identifier, TokenTriggers.None);
            ColorToken((int)Tokens.CLASSREF     , TokenType.Identifier  , TokenColor.Identifier , TokenTriggers.None);
            ColorToken((int)Tokens.NAME         , TokenType.Identifier  , TokenColor.Identifier , TokenTriggers.None);
            ColorToken((int)Tokens.REGEX        , TokenType.Literal     , TokenColor.String     , TokenTriggers.None);
            ColorToken((int)Tokens.BL_COMMENT   , TokenType.Comment     , TokenColor.Comment    , TokenTriggers.None);
            ColorToken((int)Tokens.LN_COMMENT   , TokenType.LineComment , TokenColor.Comment    , TokenTriggers.None);

            ColorToken((int)Tokens.LBRACK       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.RBRACK       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.LBRACE       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.RBRACE       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.LPAREN       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.RPAREN       , TokenType.Delimiter   , TokenColor.Text       , TokenTriggers.MatchBraces);

            //// Extra token values internal to the scanner
            ColorToken((int)Tokens.LEX_ERROR    , TokenType.Text        , TokenColor.Text                 , TokenTriggers.None);
        }
    }
}
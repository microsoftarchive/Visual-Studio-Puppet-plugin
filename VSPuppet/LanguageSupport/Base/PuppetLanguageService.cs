/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/



namespace Puppet
{
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Puppet.Parser;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using EditorClassifier;


    [Guid("90BECFDE-F4AF-4797-9519-2B5278CC18C5")]
    public class PuppetLanguageService : Microsoft.VisualStudio.Package.LanguageService
    {

        public override string GetFormatFilterList()
        {
            return "Puppet manifest (*.pp)\n*.pp";
        }

        #region IVsProvideColorableItems

        public enum PuppetTokenColor
        {
            Start = 100,
            Keyword,
            Identifier,
            String,
            Number,
            Text,
            Operator,
            Delimiter,
            Classref,
            BlockComment,
            LineComment,
            Variable,
            Regex,
            Error,
            Size = Error - Start
        }

        public struct TokenDefinition
        {
            public TokenDefinition(TokenType type, PuppetTokenColor color, TokenTriggers triggers)
            {
                this.TokenType = type;
                this.TokenColor = color;
                this.TokenTriggers = triggers;
            }

            public TokenType TokenType;
            public PuppetTokenColor TokenColor;
            public TokenTriggers TokenTriggers;
        }

        private readonly TokenDefinition defaultDefinition = new TokenDefinition(TokenType.Text, PuppetTokenColor.Text, TokenTriggers.None);

        private readonly Dictionary<int, TokenDefinition> definitions = new Dictionary<int, TokenDefinition>();

        public void AddTokenDefinition(int token, TokenType type, PuppetTokenColor color, TokenTriggers trigger)
        {
            if (!definitions.ContainsKey(token))
            {
                definitions.Add(token, new TokenDefinition(type, color, trigger));
            }
        }

        public TokenDefinition GetTokenDefinition(int token)
        {
            TokenDefinition result;
            return definitions.TryGetValue(token, out result) ? result : defaultDefinition;
        }

        public PuppetLanguageService() : base()
        {
            InitCustomColors();
            AddTokenDefinitions();
        }

        private readonly Dictionary<int, ColorableItem> colorableItems = new Dictionary<int, ColorableItem>();
        public Dictionary<int, ColorableItem> ColorableItems
        {
            get { return colorableItems; }
        }

        private void InitCustomColor(PuppetTokenColor id, string name, string dispname, COLORINDEX foreground, COLORINDEX background, bool bold = false, bool strikethrough = false)
        {
            var fontFlags = (uint)FONTFLAGS.FF_DEFAULT;

            if (bold)
                fontFlags = fontFlags | (uint)FONTFLAGS.FF_BOLD;
            if (strikethrough)
                fontFlags = fontFlags | (uint)FONTFLAGS.FF_STRIKETHROUGH;

            ColorableItems.Add((int)id, new ColorableItem(name, dispname, foreground, background, System.Drawing.Color.Empty, System.Drawing.Color.Empty, (FONTFLAGS)fontFlags));
        }

        private void InitCustomColors()
        {
            InitCustomColor(PuppetTokenColor.Keyword, Constants.KeywordName, Constants.KeywordDisplayName, COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Identifier, Constants.IdentifierName, Constants.IdentifierDisplayName, COLORINDEX.CI_BROWN, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.String, Constants.StringName, Constants.StringDisplayName, COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Number, Constants.NumberName, Constants.NumberDisplayName, COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Text, Constants.TextName, Constants.TextDisplayName, COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Operator, Constants.OperatorName, Constants.OperatorDisplayName, COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Delimiter, Constants.DelimiterName, Constants.DelimiterDisplayName, COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.BlockComment, Constants.BlockCommentName, Constants.BlockCommentDisplayName, COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.LineComment, Constants.LineCommentName, Constants.LineCommentDisplayName, COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Variable, Constants.VariableName, Constants.VariableDisplayName, COLORINDEX.CI_PURPLE, COLORINDEX.CI_USERTEXT_BK);
            InitCustomColor(PuppetTokenColor.Regex, Constants.RegexName, Constants.RegexDisplayName, COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK, false, false);
            InitCustomColor(PuppetTokenColor.Classref, Constants.ClassrefName, Constants.BlockCommentDisplayName, COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK, false, false);
            InitCustomColor(PuppetTokenColor.Error, Constants.ErrorName, Constants.ErrorDisplayName, COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, false);

            if ((int)PuppetTokenColor.Size != ColorableItems.Count)
            {
                throw new IndexOutOfRangeException("ColorableItems");
            }
        }

        private void AddTokenDefinitions()
        {
            foreach (var kw in Lexer.Scanner.Keywords)
            {
                AddTokenDefinition((int)kw.Value, TokenType.Keyword, PuppetTokenColor.Keyword, TokenTriggers.None);
            }

            AddTokenDefinition((int)Tokens.NUMBER, TokenType.Literal, PuppetTokenColor.Number, TokenTriggers.None);

            AddTokenDefinition((int)Tokens.STRING, TokenType.String, PuppetTokenColor.String, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.VARIABLE, TokenType.Identifier, PuppetTokenColor.Variable, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.CLASSREF, TokenType.Identifier, PuppetTokenColor.Classref, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.NAME, TokenType.Identifier, PuppetTokenColor.Identifier, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.REGEX, TokenType.Literal, PuppetTokenColor.Regex, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.BL_COMMENT, TokenType.Comment, PuppetTokenColor.BlockComment, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LN_COMMENT, TokenType.LineComment, PuppetTokenColor.LineComment, TokenTriggers.None);

            AddTokenDefinition((int)Tokens.LBRACK, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);
            AddTokenDefinition((int)Tokens.RBRACK, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);
            AddTokenDefinition((int)Tokens.LBRACE, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);
            AddTokenDefinition((int)Tokens.RBRACE, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);
            AddTokenDefinition((int)Tokens.LPAREN, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);
            AddTokenDefinition((int)Tokens.RPAREN, TokenType.Delimiter, PuppetTokenColor.Delimiter, TokenTriggers.MatchBraces);

            AddTokenDefinition((int)Tokens.COLON, TokenType.Operator, PuppetTokenColor.Delimiter, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.COMMA, TokenType.Operator, PuppetTokenColor.Delimiter, TokenTriggers.None);
            
            AddTokenDefinition((int)Tokens.EQUALS, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.DIV, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.APPENDS, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.DELETES, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.ISEQUAL, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.NOTEQUAL, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.MATCH, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.NOMATCH, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.GREATEREQUAL, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.GREATERTHAN, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LESSEQUAL, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LESSTHAN, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.FARROW, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.PARROW, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LSHIFT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LLCOLLECT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.LCOLLECT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.RSHIFT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.RRCOLLECT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.RCOLLECT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.PLUS, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.MINUS, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.TIMES, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.MODULO, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.NOT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.DOT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.PIPE, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.AT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.ATAT, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.SEMIC, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.QMARK, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.TILDE, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.IN_EDGE, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.IN_EDGE_SUB, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.OUT_EDGE, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);
            AddTokenDefinition((int)Tokens.OUT_EDGE_SUB, TokenType.Operator, PuppetTokenColor.Operator, TokenTriggers.None);

            //// Extra token values internal to the scanner
            AddTokenDefinition((int)Tokens.LEX_ERROR, TokenType.Text, PuppetTokenColor.Error, TokenTriggers.None);
        }

        public override int GetColorableItem(int index, out IVsColorableItem item)
        {

            if (Enum.IsDefined(typeof (PuppetTokenColor), index)
                && ColorableItems.ContainsKey(index))
            {
                item = ColorableItems[index];
                return Microsoft.VisualStudio.VSConstants.S_OK;
            }

            return base.GetColorableItem(index, out item);
        }

        public override int GetItemCount(out int count)
        {
            count = ColorableItems.Count;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion IVsProvideColorableItems

        #region MPF Accessor and Factory specialisation

        private LanguagePreferences preferences;
        
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (this.preferences == null)
            {
                this.preferences = 
                    new LanguagePreferences(this.Site,
                        typeof(PuppetLanguageService).GUID,
                        this.Name);

                this.preferences.Init();
            }

            return this.preferences;
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            return new PuppetSource(this, buffer, this.GetColorizer(buffer));
        }

        private IScanner scanner;

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (scanner == null)
                this.scanner = new LineScanner(this);

            return this.scanner;
        }
        #endregion

        public override void OnIdle(bool periodic)
        {
            // from IronPythonLanguage sample
            // this appears to be necessary to get a parse request with ParseReason = Check?
            Source src = (Source) GetSource(this.LastActiveTextView);
            if (src != null && src.LastParseTime >= Int32.MaxValue >> 12)
            {
                src.LastParseTime = 0;
            }

            base.OnIdle(periodic);
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            PuppetSource source = (PuppetSource)this.GetSource(req.FileName);
            bool yyparseResult = false;

            // req.DirtySpan seems to be set even though no changes have occurred
            // source.IsDirty also behaves strangely
            // might be possible to use source.ChangeCount to sync instead

            if (req .Sink.Reason == ParseReason.Check || req.DirtySpan.iStartIndex != req.DirtySpan.iEndIndex
                || req.DirtySpan.iStartLine != req.DirtySpan.iEndLine)
            {
                Puppet.Parser.ErrorHandler handler = new Parser.ErrorHandler();
                Puppet.Lexer.Scanner scanner = new Lexer.Scanner(); // string interface
                Parser.Parser parser = new PuppetParser();  // use noarg constructor

                parser.scanner = scanner;
                scanner.Handler = handler;
                parser.SetHandler(handler);
                scanner.SetSource(req.Text, 0);

                parser.MBWInit(req);
                yyparseResult = parser.Parse();

                // store the parse results
                // source.ParseResult = aast;
                source.ParseResult = null;
                source.Braces = parser.Braces;

                // for the time being, just pull errors back from the error handler
                if (handler.ErrNum > 0)
                {
                    foreach (Parser.Error error in handler.SortedErrorList())
                    {
                        var span = new TextSpan();
                        span.iStartLine = span.iEndLine = error.line - 1;
                        span.iStartIndex = error.column;
                        span.iEndIndex = error.column + error.length;
                        req.Sink.AddError(req.FileName, error.message, span, Severity.Error);
                    }
                }
            }

            switch (req.Reason)
            {
                case ParseReason.Check:
                case ParseReason.HighlightBraces:
                case ParseReason.MatchBraces:
                case ParseReason.MemberSelectAndHighlightBraces:
                    int indexOfCaret = source.GetPositionOfLineIndex(req.Line, req.Col);
                    if (source.Braces != null)
                    {
                        foreach (TextSpan[] brace in source.Braces)
                        {
                            if (brace.Length == 2)
                                req.Sink.MatchPair(brace[0], brace[1], 1);
                            else if (brace.Length >= 3)
                                req.Sink.MatchTriple(brace[0], brace[1], brace[2], 1);
                        }
                    }
                    break;
                case ParseReason.QuickInfo:
                    return new PuppetAuthoringScope(this.GetSource(req.FileName)); 
                default:
                    break;
            }

            return new PuppetAuthoringScope(source.ParseResult);
        }

        public override string Name
        {
            get { return Configuration.Name; }
        }
    }
}
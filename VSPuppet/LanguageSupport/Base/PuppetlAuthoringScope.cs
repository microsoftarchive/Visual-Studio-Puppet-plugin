/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.TextManager.Interop;

    public class PuppetAuthoringScope : AuthoringScope
    {
        private object parseResult;
        private IASTResolver resolver;
        private Source source;

        public PuppetAuthoringScope(Source source)
        {
             this.source = source;
            // how should this be set?
            this.resolver = new Resolver();
        }

        public PuppetAuthoringScope(object parseResult)
        {
            this.parseResult = parseResult;
        
            // how should this be set?
            this.resolver = new Resolver();
        }


        // ParseReason.QuickInfo
        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            var tokingInfo = this.source.GetTokenInfo(line, col+1);
            span = new TextSpan();
            span.iStartLine = line;
            span.iEndLine = line;
            span.iStartIndex = tokingInfo.StartIndex;
            span.iEndIndex = tokingInfo.EndIndex + 1;

            var tokenFound = this.source.GetText(span);

            Lexer.Scanner.TokDesc desc = Lexer.Scanner.GetTokDesc((Parser.Tokens)tokingInfo.Token);

            if (desc != null)
                return desc.Remark;

            return string.Empty;
        }

        // ParseReason.CompleteWord
        // ParseReason.DisplayMemberList
        // ParseReason.MemberSelect
        // ParseReason.MemberSelectAndHilightBraces
        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
                string tokenText;
                var hr = view.GetTextStream(line, info.StartIndex, line, col, out tokenText);
                
                IList<Declaration> declarations;
            switch (reason)
            {
            case ParseReason.CompleteWord:
                        declarations = resolver.FindCompletions(tokenText, line, col);
                break;
            case ParseReason.DisplayMemberList:
            case ParseReason.MemberSelect:
            case ParseReason.MemberSelectAndHighlightBraces:
                declarations = resolver.FindMembers(parseResult, line, col);
                break;
            default:
                throw new ArgumentException("reason");
            }

                return new PuppetDeclarations(declarations);
        }

        // ParseReason.GetMethods
        public override Methods GetMethods(int line, int col, string name)
        {
                return new PuppetMethods(resolver.FindMethods(parseResult, line, col, name));
        }

        // ParseReason.Goto
        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            // throw new System.NotImplementedException();
            span = new TextSpan();
            return null;
        }
    }
}
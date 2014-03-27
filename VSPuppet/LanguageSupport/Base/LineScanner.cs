// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet
{
    using Microsoft.VisualStudio.Package;
    using Puppet.Parser;

    /// <summary>
    /// LineScanner wraps the GPLEX scanner to provide the IScanner interface
    /// required by the Managed Package Framework. This includes mapping tokens
    /// to color definitions.
    /// </summary>
    public class LineScanner : IScanner
    {
        Puppet.ParserGenerator.IColorScan lex = null;
        private PuppetLanguageService service;

        public LineScanner(PuppetLanguageService service)
        {
            this.lex = new Puppet.Lexer.Scanner();
            this.service = service;
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            int start, end;
            int token = lex.GetNext(ref state, out start, out end);

            // !EOL and !EOF
            if (token != (int)Tokens.EOF)
            {
                var definition = this.service.GetTokenDefinition(token);
                tokenInfo.StartIndex = start;
                tokenInfo.EndIndex = end;
                tokenInfo.Color = (TokenColor)definition.TokenColor;
                tokenInfo.Type = definition.TokenType;
                tokenInfo.Trigger = definition.TokenTriggers;
                tokenInfo.Token = token;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetSource(string source, int offset)
        {
            lex.SetSource(source, offset);
        }
    }
}
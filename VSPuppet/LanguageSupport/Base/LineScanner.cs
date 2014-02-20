/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System;
    using Puppet.Parser;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// LineScanner wraps the GPLEX scanner to provide the IScanner interface
    /// required by the Managed Package Framework. This includes mapping tokens
    /// to color definitions.
    /// </summary>
    public class LineScanner : IScanner
    {
        Puppet.ParserGenerator.IColorScan lex = null;

        public LineScanner()
        {
            this.lex = new Puppet.Lexer.Scanner();
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            int start, end;
            int token = lex.GetNext(ref state, out start, out end);

            // !EOL and !EOF
            if (token != (int)Tokens.EOF)
            {
                Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
                tokenInfo.StartIndex = start;
                tokenInfo.EndIndex = end;
                tokenInfo.Color = definition.TokenColor;
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
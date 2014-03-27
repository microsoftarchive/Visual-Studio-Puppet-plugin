// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet
{

    class PuppetParser : Puppet.Parser.Parser
    {
        protected override string TerminalToString(int terminal)
        {
            Lexer.Scanner.TokDesc desc = Lexer.Scanner.GetTokDesc((Parser.Tokens)terminal);

            if (desc != null)
            {
                return "'" + desc.Remark + "'";
            }

            return base.TerminalToString(terminal);
        }

    }
}

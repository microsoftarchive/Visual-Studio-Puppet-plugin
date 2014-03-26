// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace Puppet
{
    using Puppet.Parser;

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

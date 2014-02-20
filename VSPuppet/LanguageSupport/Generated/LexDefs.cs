/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet.Parser
{
    using System;
    //
    // These are the dummy declarations for stand-alone lex applications
    // normally these declarations would come from the parser.
    // 

    public interface IErrorHandler
    {
        int ErrNum { get; }
        int WrnNum { get; }
        void AddError(string msg, int lin, int col, int len, int severity);
    }
}

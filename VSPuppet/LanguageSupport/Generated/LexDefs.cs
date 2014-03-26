// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
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

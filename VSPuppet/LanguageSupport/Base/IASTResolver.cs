// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace Puppet
{
    using System;
    using System.Collections.Generic;

    interface IASTResolver
    {
        IList<Declaration> FindCompletions(object result, int line, int col);
        IList<Declaration> FindMembers(object result, int line, int col);
        string FindQuickInfo(object result, int line, int col);
        IList<Method> FindMethods(object result, int line, int col, string name);
    }
}
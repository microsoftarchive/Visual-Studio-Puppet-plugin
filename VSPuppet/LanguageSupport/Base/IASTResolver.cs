// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet
{
    using System.Collections.Generic;

    interface IASTResolver
    {
        IList<Declaration> FindCompletions(object result, int line, int col);
        IList<Declaration> FindMembers(object result, int line, int col);
        string FindQuickInfo(object result, int line, int col);
        IList<Method> FindMethods(object result, int line, int col, string name);
    }
}
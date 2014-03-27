// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet
{
    using Microsoft.VisualStudio.Package;
    using System.Collections.Generic;
    using System.Linq;

    public class Resolver : IASTResolver
    {
        #region IASTResolver Members

        public IList<Declaration> FindCompletions(object result, int line, int col)
        {
            var text = result.ToString();

            return (
                from kw in Lexer.Scanner.TokenDescMap.Where(td => td.Value.TokenType == TokenType.Keyword) 
                where kw.Key.StartsWith(text)
                orderby kw.Key
                select new Declaration("", kw.Key, 5, kw.Key)
                ).ToList();
        }

        public IList<Declaration> FindMembers(object result, int line, int col)
        {
            var members = new List<Declaration>();

            return members;
        }

        public string FindQuickInfo(object result, int line, int col)
        {
            return "unknown";
        }

        public IList<Method> FindMethods(object result, int line, int col, string name)
        {
            return new List<Method>();
        }

        #endregion
    }
}

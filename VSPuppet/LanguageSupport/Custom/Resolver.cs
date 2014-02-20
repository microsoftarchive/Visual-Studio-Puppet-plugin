/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Resolver : IASTResolver
    {
        #region IASTResolver Members

        public IList<Declaration> FindCompletions(object result, int line, int col)
        {
            var declarations = new List<Declaration>();

            var text = result.ToString();

            foreach (var kw in Lexer.Scanner.Keywords)
            {
                if (kw.Key.StartsWith(text))
                {
                    declarations.Add(new Declaration("", kw.Key, 5, kw.Key));
                }
            }

            return declarations;
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

// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet.ParserGenerator
{
    public class Rule
    {
        public int lhs; // symbol
        public int[] rhs; // symbols

        public Rule(int lhs, int[] rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }
    }
}

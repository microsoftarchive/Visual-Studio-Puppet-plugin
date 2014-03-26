// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace Puppet
{
    using System;
    using System.Collections.Generic;

    public struct Declaration
    {
        public Declaration(string description, string displayText, int glyph, string name)
        {
            this.Description = description;
            this.DisplayText = displayText;
            this.Glyph = glyph;
            this.Name = name;
        }

        public string Description;
        public string DisplayText;
        public int Glyph;
        public string Name;
    }
}
/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

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
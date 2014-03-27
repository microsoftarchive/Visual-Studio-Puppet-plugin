// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved.
//     Licensed under the Apache License, Version 2.0.
//     See License.txt in the project root for license information
// --------------------------------------------------------------------------
namespace Puppet
{

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
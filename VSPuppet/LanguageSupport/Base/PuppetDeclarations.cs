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

    public class PuppetDeclarations : Declarations
    {
        IList<Declaration> declarations;

        public PuppetDeclarations(IList<Declaration> declarations)
        {
            this.declarations = declarations;
        }

        public override int GetCount()
        {
            return declarations.Count;
        }

        public override string GetDescription(int index)
        {
            return declarations[index].Description;
        }

        public override string GetDisplayText(int index)
        {
            return declarations[index].DisplayText;
        }

        public override int GetGlyph(int index)
        {
            return declarations[index].Glyph;
        }

        public override string GetName(int index)
        {
            if (index >= 0)
                return declarations[index].Name;

            return null;
        }
    }
}
/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Package;

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